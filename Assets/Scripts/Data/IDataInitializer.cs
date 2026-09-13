using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

public interface IDataInitializer {
    string LoaderId { get; }

    int LoadPriority { get; }

    UniTask InitializeAsync(CancellationToken token = default);

    bool IsLoaded { get; }

    UniTask ResetAsync(CancellationToken token = default);
}

public interface IDataSource<T> {
    T Get();
}


/// <summary>
/// Универсальный провайдер. Загружает ассет, распаковывает данные и хранит их в памяти.
/// </summary>
public sealed class AssetDataLoader<T> :
    IDataSource<T>,
    IDataInitializer
    where T : UnityEngine.Object {
    private readonly string _assetPath;

    private T _asset;

    public string LoaderId => $"Asset_{typeof(T).Name}";
    public int LoadPriority { get; }
    public bool IsLoaded => _asset != null;

    public AssetDataLoader(string assetPath, int priority = 10) {
        _assetPath = assetPath;
        LoadPriority = priority;
    }

    public async UniTask InitializeAsync(CancellationToken token = default) {
        if (IsLoaded)
            return;

        var request = Resources.LoadAsync<T>(_assetPath);

        await request.ToUniTask(cancellationToken: token);

        if (request.asset == null) {
            Debug.LogWarning($"LoaderId: {LoaderId} Can't load by AssetPath: Resources/{_assetPath}");
            return;
        }

        _asset = (T)request.asset;
    }

    public T Get() {
        if (!IsLoaded)
            Debug.LogWarning("Attempt to get not initialized data");

        return _asset;
    }

    public UniTask ResetAsync(CancellationToken token = default) {
        _asset = null;

        return UniTask.CompletedTask;
    }
}