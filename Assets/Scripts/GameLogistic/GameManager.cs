using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] DataLoaderRegistry dataLoaderRegistry;
    
    [Inject] QuizBoardController controller;

    public void Start() {
        Debug.Log($"GameManager initialized. And have {dataLoaderRegistry}");
        StartGame().Forget();
    }

    

    private async UniTask StartGame() {
        await dataLoaderRegistry.LoadAllAsync();

        controller.ShowBoard(5);
    }
}
