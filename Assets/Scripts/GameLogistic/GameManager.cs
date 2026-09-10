using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] DataLoaderRegistry dataLoaderRegistry;
    [Inject] IDataSource<QuestionDatabase> dataProvider;
    [Inject] IDataSource<TestData> testDataProvider;
    [Inject] IQuestionProvider questionProvider;
    public void OnEnable() {
        Debug.Log($"GameManager initialized. And have {dataLoaderRegistry}");
        Test().Forget();
    }

    private async UniTask Test() {
        await dataLoaderRegistry.LoadAllAsync();

        QuestionDatabase questionData = dataProvider.Get();
        System.Collections.Generic.List<QuestionData> questions = questionProvider.GetRandomQuestions(25);
        Debug.Log($"Retrieved {questions.Count} random questions.");
        TestData testData = testDataProvider.Get();
        Debug.Log(testData.ToString());
    }
}
