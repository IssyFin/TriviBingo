using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using UnityEngine;

public class DataLoaderRegistry {
    private readonly List<IDataInitializer> _loaders = new();
    private UniTaskCompletionSource _loadingCompletion;

    public IReadOnlyList<IDataInitializer> Loaders => _loaders;
    public bool IsAllLoaded => _loaders.All(l => l.IsLoaded);

    public DataLoaderRegistry(IEnumerable<IDataInitializer> loaders) {
        _loaders.AddRange(loaders);
        _loaders.Sort(
            (a, b) => a.LoadPriority.CompareTo(b.LoadPriority)
        );
    }

    public void RegisterLoader(IDataInitializer loader) {
        if (_loaders.Contains(loader))
            return;

        _loaders.Add(loader);
        // Сортируем по приоритету
        _loaders.Sort((a, b) => a.LoadPriority.CompareTo(b.LoadPriority));

        Debug.Log($"[DataLoaderRegistry] Registered: {loader.LoaderId} (Priority: {loader.LoadPriority})");
    }

    public void RegisterLoaders(params IDataInitializer[] loaders) {
        foreach (var loader in loaders)
            RegisterLoader(loader);
    }

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
                var tasks = group.Select(l => l.InitializeAsync(token));
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

    public T GetLoader<T>() where T : IDataInitializer {
        return _loaders.OfType<T>().FirstOrDefault();
    }

    public IDataInitializer GetLoader(string loaderId) {
        return _loaders.FirstOrDefault(l => l.LoaderId == loaderId);
    }

    public async UniTask ResetAllAsync(CancellationToken token = default) {
        var tasks = _loaders.Select(l => l.ResetAsync(token));
        await UniTask.WhenAll(tasks);
    }
}

