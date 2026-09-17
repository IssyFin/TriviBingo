using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class AnswerButton : MonoBehaviour {
    [SerializeField] private Button button;
    [SerializeField] private TextMeshProUGUI label;

    public event Action<Answer> Clicked;

    private Answer answer;

    private void Awake() {
        if (button == null) button = GetComponent<Button>();
        button.onClick.AddListener(HandleClick);
    }

    private void OnDestroy() {
        button.onClick.RemoveListener(HandleClick);
    }

    public void Setup(Answer answer) {
        this.answer = answer;
        label.text = answer.Text;
    }

    public void SetInteractable(bool value) => button.interactable = value;

    private void HandleClick() => Clicked?.Invoke(answer);
}