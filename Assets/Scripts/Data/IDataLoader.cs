using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;
/// <summary>
/// Базовый интерфейс для всех загрузчиков данных
/// </summary>
public interface IDataLoader {
    /// <summary>
    /// Уникальный идентификатор загрузчика (для логирования и отладки)
    /// </summary>
    string LoaderId { get; }

    /// <summary>
    /// Приоритет загрузки (меньше = раньше)
    /// </summary>
    int LoadPriority { get; }

    /// <summary>
    /// Загрузка данных
    /// </summary>
    UniTask LoadDataAsync(CancellationToken token = default);

    /// <summary>
    /// Проверка, загружены ли данные
    /// </summary>
    bool IsLoaded { get; }

    /// <summary>
    /// Сброс данных (для перезагрузки уровня и т.д.)
    /// </summary>
    UniTask ResetAsync(CancellationToken token = default);
}

public class DataLoaderRegistry {
    private readonly List<IDataLoader> _loaders = new();
    private bool _isLoading = false;
    private UniTaskCompletionSource _loadingCompletion = new();

    public IReadOnlyList<IDataLoader> Loaders => _loaders;
    public bool IsLoading => _isLoading;
    public bool IsAllLoaded => _loaders.All(l => l.IsLoaded);

    /// <summary>
    /// Регистрация загрузчика
    /// </summary>
    public void RegisterLoader(IDataLoader loader) {
        if (_loaders.Contains(loader))
            return;

        _loaders.Add(loader);
        // Сортируем по приоритету
        _loaders.Sort((a, b) => a.LoadPriority.CompareTo(b.LoadPriority));

        Debug.Log($"[DataLoaderRegistry] Registered: {loader.LoaderId} (Priority: {loader.LoadPriority})");
    }

    /// <summary>
    /// Регистрация нескольких загрузчиков
    /// </summary>
    public void RegisterLoaders(params IDataLoader[] loaders) {
        foreach (var loader in loaders)
            RegisterLoader(loader);
    }

    /// <summary>
    /// Загрузка всех данных
    /// </summary>
    public async UniTask LoadAllAsync(CancellationToken token = default) {
        if (_loadingCompletion != null) {
            await _loadingCompletion.Task;
            return;
        }

        if (IsAllLoaded)
            return;

        _loadingCompletion = new UniTaskCompletionSource();

        try {
            var groups = _loaders
                .GroupBy(l => l.LoadPriority)
                .OrderBy(g => g.Key);

            foreach (var group in groups) {
                var tasks = group.Select(l => l.LoadDataAsync(token));
                await UniTask.WhenAll(tasks);
            }

            _loadingCompletion.TrySetResult();
        } catch (Exception ex) {
            _loadingCompletion.TrySetException(ex);
            throw;
        } finally {
            _loadingCompletion = null;
        }
    }

    /// <summary>
    /// Получение конкретного загрузчика по типу
    /// </summary>
    public T GetLoader<T>() where T : IDataLoader {
        return _loaders.OfType<T>().FirstOrDefault();
    }

    /// <summary>
    /// Получение загрузчика по ID
    /// </summary>
    public IDataLoader GetLoader(string loaderId) {
        return _loaders.FirstOrDefault(l => l.LoaderId == loaderId);
    }

    /// <summary>
    /// Сброс всех загрузчиков
    /// </summary>
    public async UniTask ResetAllAsync(CancellationToken token = default) {
        var tasks = _loaders.Select(l => l.ResetAsync(token));
        await UniTask.WhenAll(tasks);
    }
}