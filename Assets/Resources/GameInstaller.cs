using DataManagement;
using UnityEngine;
using Zenject;

public class GameInstaller : MonoInstaller {
    [SerializeField] private TextAsset questionsJsonFile;

    public override void InstallBindings() {
        Container.Bind<IDataSerializer>().To<NewtonsoftSerializer>().AsSingle();
        Container.Bind<IFileStorage>().To<FileStorage>().AsSingle().WithArguments(Application.persistentDataPath);
        Container.Bind<DataStore>().AsSingle();

        Container.Bind<DataLoaderRegistry>().AsSingle();

        Container.Bind<IQuestionProvider>().To<QuestionProvider>().AsSingle();
    }
}

public static class DataPaths {
    public const string Questions = "questions.json";
}