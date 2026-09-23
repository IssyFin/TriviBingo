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

    [Header ("UI")]
    public UIRoot _uiRootPrefab;
    [SerializeField] private UIWindowsConfig _windowsConfig;
    [SerializeField] private UIHudConfig _hudConfig;
    public override void InstallBindings() {
        BindDataLoading();

        Container.Bind<GameManager>().FromNewComponentOnNewGameObject().AsSingle();
        Container.BindInterfacesAndSelfTo<QuizInputRouter>().AsSingle();

        Container.BindInterfacesAndSelfTo<BoardPresenter>().AsSingle().WithArguments(boardView);
        Container.BindInterfacesAndSelfTo<QuizGameController>().AsSingle();

        Container.Bind<IEnvelopeFactory>().To<EnvelopeFactory>().AsSingle().WithArguments(envelopeViewPrefab);
        Container.Bind<ICategoryColorProvider>().To<CategoryColorProvider>().AsSingle().WithArguments(categoryColorConfig);

        BindInputSystem();
        BindInteractionSystem();
        BindUI();
    }

    private void BindDataLoading() {
        Container.Bind<IDataSerializer>().To<NewtonsoftJsonSerializer>().AsSingle();
        Container.Bind<IFileStorage>().To<FileStorage>().AsSingle().WithArguments(Application.persistentDataPath);
        Container.Bind<DataStore>().AsSingle();

        Container.Bind<DataLoaderRegistry>().AsSingle();

        Container.BindInterfacesAndSelfTo<QuestionRepository>().AsSingle();
        Container.BindInterfacesAndSelfTo<QuestionProvider>().AsSingle();

        Container.BindAssetLoader<QuestionDatabase>(DataPaths.Questions);
        Container.BindAssetLoader<TestData>(DataPaths.TestDatas);
    }

    private void BindUI() {
        // Core

        Container.Bind<UIRoot>()
            .FromComponentInNewPrefab(_uiRootPrefab)
            .AsSingle();

        Container.Bind<IWindowService>().To<UIWindowService>().AsSingle().WithArguments(_windowsConfig);
        Container.Bind<IHudService>().To<HudService>().AsSingle().WithArguments(_hudConfig);


        //Add
        Container.Bind<IQuizService>().To<QuizService>().AsSingle();

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
