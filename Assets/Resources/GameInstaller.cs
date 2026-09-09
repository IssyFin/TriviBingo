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

        //Container.Bind<IQuestionProvider>().To<QuestionProvider>().AsSingle().WithArguments(DataPaths.Questions);
        Container.BindInterfacesAndSelfTo<QuestionProvider>().AsSingle();

        Container.BindInterfacesAndSelfTo<AssetDataLoader<QuestionData>>().AsSingle().WithArguments(DataPaths.Questions);
    }
}

public static class DataPaths {
    public const string Questions = "questions";
}