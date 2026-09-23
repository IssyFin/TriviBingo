using Cysharp.Threading.Tasks;
using UnityEngine;
using UnityEngine.InputSystem.XR;
using Zenject;

public class GameManager : MonoBehaviour
{
    [Inject] DataLoaderRegistry dataLoaderRegistry;
    
    [Inject] QuizGameController controller;

    public void Start() {
        Debug.Log($"GameManager initialized. And have {dataLoaderRegistry}");
        StartGame().Forget();
    }

    

    private async UniTask StartGame() {
        await dataLoaderRegistry.LoadAllAsync();

        controller.StartGame(5);
    }
}
