using DataManagement;
using UnityEditor.SearchService;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {

    public CategoryColorConfig categoryColorConfig;
    public QuestionEnvelopeView envelopeViewPrefab;
    public BoardInteractionService boardInteractionService;
    public CursorObjectDetector objectDetector;
    public BoardView boardView;
    public override void InstallBindings() {
        Container.Bind<GameManager>().FromNewComponentOnNewGameObject().AsSingle();

        Container.Bind<IDataSerializer>().To<NewtonsoftJsonSerializer>().AsSingle();
        Container.Bind<IFileStorage>().To<FileStorage>().AsSingle().WithArguments(Application.persistentDataPath);
        Container.Bind<DataStore>().AsSingle();

        Container.Bind<DataLoaderRegistry>().AsSingle();

        Container.BindInterfacesAndSelfTo<QuestionRepository>().AsSingle();
        Container.BindInterfacesAndSelfTo<StubQuestionProvider>().AsSingle();

        Container.BindAssetLoader<QuestionDatabase>(DataPaths.Questions);
        Container.BindAssetLoader<TestData>(DataPaths.TestDatas);

        Container.BindInterfacesAndSelfTo<QuizBoardService>().AsSingle();

        Container.Bind<BoardView>().FromComponentInNewPrefab(boardView).AsSingle();
        Container.Bind<IEnvelopeFactory>().To<EnvelopeFactory>().AsSingle().WithArguments(envelopeViewPrefab);
        Container.Bind<ICategoryColorProvider>().To<CategoryColorProvider>().AsSingle().WithArguments(categoryColorConfig);
        BindInputSystem();
        BindInteractionSystem();
    }

    private void BindInteractionSystem() {
        Container.BindInterfacesAndSelfTo<BoardInteractionService>().FromComponentInNewPrefab(boardInteractionService).AsSingle();
        Container.BindInterfacesAndSelfTo<IObjectDetector>().FromComponentInNewPrefab(objectDetector).AsSingle();
    }

    private void BindInputSystem() {
        Container.BindInterfacesAndSelfTo<InputService>().AsSingle();
        Container.BindInterfacesAndSelfTo<UIInputReader>().AsSingle();
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
