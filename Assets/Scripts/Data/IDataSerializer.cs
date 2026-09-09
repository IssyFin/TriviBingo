using Cysharp.Threading.Tasks;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading;
using System.Linq;
using UnityEngine;

namespace DataManagement {
    /// <summary>
    /// Универсальная абстракция сериализатора.
    /// </summary>
    public interface IDataSerializer {
        byte[] Serialize<T>(T data);
        T Deserialize<T>(byte[] bytes);
        object Deserialize(byte[] bytes, Type type);
    }

    /// <summary>
    /// Сериализатор на Newtonsoft.Json. В отличие от JsonUtility корректно
    /// работает с Dictionary, вложенными коллекциями и т.д.
    /// </summary>
    public class NewtonsoftSerializer : IDataSerializer {
        private readonly JsonSerializerSettings _settings;

        public NewtonsoftSerializer() {
            _settings = new JsonSerializerSettings {
                Formatting = Formatting.Indented,
                // Auto включает сохранение типов только при необходимости (для полиморфизма)
                TypeNameHandling = TypeNameHandling.None,
                NullValueHandling = NullValueHandling.Ignore,
                MissingMemberHandling = MissingMemberHandling.Ignore
            };
        }

        public byte[] Serialize<T>(T data) {
            string json = JsonConvert.SerializeObject(data, _settings);
            return Encoding.UTF8.GetBytes(json);
        }

        public T Deserialize<T>(byte[] bytes) {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject<T>(json, _settings);
        }

        public object Deserialize(byte[] bytes, Type type) {
            string json = Encoding.UTF8.GetString(bytes);
            return JsonConvert.DeserializeObject(json, type, _settings);
        }
    }

    /// <summary>
    /// Отвечает ИСКЛЮЧИТЕЛЬНО за чтение/запись строк на диск по слотам.
    /// Ничего не знает про формат данных или игровую логику.
    /// </summary>
    public interface IFileStorage {
        UniTask<byte[]> ReadAsync(string path, CancellationToken token = default);
        UniTask WriteAsync(string path, byte[] content, CancellationToken token = default);
        bool Exists(string path);
        void Delete(string path);
        void CreateDirectory(string path);
        void DeleteDirectory(string path);

        IReadOnlyList<string> GetDirectories(string path);
        IReadOnlyList<string> GetFiles(string path, string searchPattern = "*");
    }

    public class FileStorage : IFileStorage {
        private readonly string _rootPath;

        public FileStorage(string rootPath) {
            _rootPath = rootPath;
        }

        private string GetFullPath(string relativePath) {
            var root = Path.GetFullPath(_rootPath);
            var combined = Path.GetFullPath(Path.Combine(root, relativePath));

            if (!combined.StartsWith(root + Path.DirectorySeparatorChar,
                StringComparison.OrdinalIgnoreCase) && combined != root) {
                throw new UnauthorizedAccessException($"Path traversal detected: {relativePath}");
            }
            return combined;
        }

        public async UniTask<byte[]> ReadAsync(string path, CancellationToken token = default) {
            var fullPath = GetFullPath(path);
            if (!File.Exists(fullPath))
                throw new FileNotFoundException($"File not found: {fullPath}");

            // UniTask поддерживает токены через .AsAsyncOperation() или прямую обертку
            return await File.ReadAllBytesAsync(fullPath, token);
        }

        public async UniTask WriteAsync(
    string path,
    byte[] content,
    CancellationToken token = default) {
            var fullPath = GetFullPath(path);
            var directory = Path.GetDirectoryName(fullPath);

            if (!string.IsNullOrEmpty(directory))
                Directory.CreateDirectory(directory);

            var tempPath = fullPath + ".tmp";
            var tempPathBackup = fullPath + ".bak";

            try {
                await WriteTempFileAsync(tempPath, content, token);

                if (File.Exists(fullPath)) {
                    // Создаем резервную копию на случай сбоя
                    File.Copy(fullPath, tempPathBackup, overwrite: true);

                    File.Replace(
                        tempPath,
                        fullPath,
                        null,
                        ignoreMetadataErrors: true);

                    // Удаляем резервную копию после успешной замены
                    if (File.Exists(tempPathBackup))
                        File.Delete(tempPathBackup);
                } else {
                    File.Move(tempPath, fullPath);
                }
            } catch {
                // Восстановление из резервной копии при сбое
                try {
                    if (File.Exists(tempPathBackup) && !File.Exists(fullPath))
                        File.Move(tempPathBackup, fullPath);
                } catch { }

                // Очистка временных файлов
                CleanupTempFile(tempPath);
                CleanupTempFile(tempPathBackup);

                throw;
            }
        }

        private async UniTask WriteTempFileAsync(
            string path,
            byte[] content,
            CancellationToken token) {
            await using var stream = new FileStream(
                path,
                FileMode.Create,
                FileAccess.Write,
                FileShare.None,
                bufferSize: 4096,
                options: FileOptions.WriteThrough | FileOptions.Asynchronous);

            await stream.WriteAsync(content, 0, content.Length, token);
            await stream.FlushAsync(token);
        }

        private void CleanupTempFile(string path) {
            try {
                if (File.Exists(path))
                    File.Delete(path);
            } catch { }
        }

        public bool Exists(string path) => File.Exists(GetFullPath(path));

        public void Delete(string path) {
            var fullPath = GetFullPath(path);
            if (File.Exists(fullPath)) {
                File.Delete(fullPath);
            }
        }

        public void CreateDirectory(string path) => Directory.CreateDirectory(GetFullPath(path));
        public void DeleteDirectory(string path) {
            var fullPath = GetFullPath(path);
            if (Directory.Exists(fullPath)) {
                Directory.Delete(fullPath, recursive: true);
            }
        }

        public IReadOnlyList<string> GetDirectories(string path) {
            var fullPath = GetFullPath(path);
            if (!Directory.Exists(fullPath)) return Array.Empty<string>();

            return Directory.GetDirectories(fullPath)
                .Select(Path.GetFileName)
                .ToList();
        }

        public IReadOnlyList<string> GetFiles(string path, string searchPattern = "*") {
            var fullPath = GetFullPath(path);
            if (!Directory.Exists(fullPath)) return Array.Empty<string>();

            return Directory.GetFiles(fullPath, searchPattern)
                .Select(Path.GetFileName)
                .ToList();
        }
    }

    /// <summary>
    /// Фасад для работы с данными. Объединяет логику сериализации и файлового хранилища.
    /// </summary>
    public class DataStore {
        private readonly IDataSerializer _serializer;
        private readonly IFileStorage _storage;

        public DataStore(IDataSerializer serializer, IFileStorage storage) {
            _serializer = serializer;
            _storage = storage;
        }

        public async UniTask SaveAsync<T>(string path, T data, CancellationToken token = default) {
            byte[] rawData = _serializer.Serialize(data);
            await _storage.WriteAsync(path, rawData, token);
        }

        public async UniTask<T> LoadAsync<T>(string path, CancellationToken token = default) {
            byte[] rawData = await _storage.ReadAsync(path, token);
            return _serializer.Deserialize<T>(rawData);
        }

        public async UniTask<T> LoadOrDefaultAsync<T>(string path, T defaultValue = default, CancellationToken token = default) where T : new() {
            if (!_storage.Exists(path)) {
                Debug.LogWarning($"[DataStore] File not found: {path}, using default value");
                return defaultValue ?? new T();
            }

            try {
                return await LoadAsync<T>(path, token);
            } catch (Exception ex) {
                Debug.LogError($"[DataStore] Failed to load {path}: {ex.Message}");
                return defaultValue ?? new T();
            }
        }
    }
}

