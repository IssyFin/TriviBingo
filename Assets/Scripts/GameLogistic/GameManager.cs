using Cysharp.Threading.Tasks;
using UnityEngine;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] DataLoaderRegistry dataLoaderRegistry;
    [Inject] IQuestionProvider questionProvider;
    [Inject] QuizBoardService quizService;

    [SerializeField] BoardView boardView;
    public void OnEnable() {
        Debug.Log($"GameManager initialized. And have {dataLoaderRegistry}");
        StartGame().Forget();
    }

    public void OnDisable() {
        boardView?.Clear();
    }

    private async UniTask StartGame() {
        await dataLoaderRegistry.LoadAllAsync();

        var board = new QuizBoard();
        board.Generate(5, questionProvider);
        quizService.ShowBoard(board);
    }
}
