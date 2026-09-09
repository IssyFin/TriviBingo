using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] DataLoaderRegistry dataLoaderRegistry;
    [Inject] IDataSource<QuestionData> dataProvider;
    [Inject] IQuestionProvider questionProvider;
    public void OnEnable() {
        Debug.Log($"GameManager initialized. And have {dataLoaderRegistry}");
        Test().Forget();
    }

    private async UniTask Test() {
        await dataLoaderRegistry.LoadAllAsync();

        QuestionData questionData = dataProvider.Get();
        System.Collections.Generic.List<Question> questions = questionProvider.GetQuestions(25);
        Debug.Log(questions.ToString());
;
    }
}
