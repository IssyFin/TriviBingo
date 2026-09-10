using DataManagement;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {

    public override void InstallBindings() {
        Container.Bind<GameManager>().FromNewComponentOnNewGameObject().AsSingle();

        Container.Bind<IDataSerializer>().To<NewtonsoftJsonSerializer>().AsSingle();
        Container.Bind<IFileStorage>().To<FileStorage>().AsSingle().WithArguments(Application.persistentDataPath);
        Container.Bind<DataStore>().AsSingle();

        Container.Bind<DataLoaderRegistry>().AsSingle();

        Container.BindInterfacesAndSelfTo<QuestionRepository>().AsSingle();
        Container.BindInterfacesAndSelfTo<QuestionProvider>().AsSingle();

        Container.BindAssetLoader<QuestionDatabase>(DataPaths.Questions);
        Container.BindAssetLoader<TestData>(DataPaths.TestDatas);
    }
}

public static class DataLoaderBindingExtensions {
    public static void BindAssetLoader<T>(
        this DiContainer container,
        string assetPath,
        int priority = 10)
        where T : UnityEngine.Object {
        container.BindInterfacesAndSelfTo<AssetDataLoader<T>>()
            .AsSingle()
            .WithArguments(assetPath, priority);
    }
}

public static class DataPaths {
    public const string Questions = "questions";
    public const string TestDatas = "TestData";
}
