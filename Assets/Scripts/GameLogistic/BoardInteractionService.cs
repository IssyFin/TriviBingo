using System;
using UnityEngine;
using Zenject;

public class BoardInteractionService : MonoBehaviour, IInteractionService {

    private GameObject _current;

    public event Action<GameObject> Selected;
    public event Action<GameObject, bool> HoverChanged;

    private IObjectDetector detector;
    private UIInputReader input;

    [Inject]
    public void Construct(UIInputReader uIInputReader, IObjectDetector objectDetector) {
        input = uIInputReader;
        detector = objectDetector;
    }

    private void OnEnable() {
        if (input == null) return;
        input.OnClick += HandleClick;
    }
    private void OnDisable() {
        if (input == null) return;
        input.OnClick -= HandleClick;
    }

    private void Update() {
        detector.TryDetectObject(out var result);
        ChangeCurrent(result.GameObject);
    }

    private void HandleClick() {
        if (_current != null) Selected?.Invoke(_current);
    }

    private void ChangeCurrent(GameObject foundObject) {
        if (_current == foundObject) return;

        if (_current != null) HoverChanged?.Invoke(_current, false);
        _current = foundObject;
        if (_current != null) HoverChanged?.Invoke(_current, true);
    }
}
