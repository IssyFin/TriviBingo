using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class QuestionWindow : UIWindow {
    [SerializeField] private AnswerButton answerButtonPrefab;
    [SerializeField] private TextMeshProUGUI questionField;
    [SerializeField] private Transform answersContainer;

    private readonly List<AnswerButton> answerButtons = new();

    public event Action<Answer> OnAnswerReceived;

    public void SetQuestion(QuestionData question) {
        ClearQuestioning();

        if (questionField != null)
            questionField.text = question.Text;

        foreach (var answer in question.Answers) {
            if (string.IsNullOrEmpty(answer.Text)) {
                Debug.LogWarning($"Detected empty answer of : " + question);
                continue;
            }

            AnswerButton button = Instantiate(answerButtonPrefab, answersContainer);
            button.Setup(answer);
            button.Clicked += HandleAnswerClicked;
            answerButtons.Add(button);
        }
    }

    private void HandleAnswerClicked(Answer answer) {
        // Опционально: блокируем все кнопки, чтобы нельзя было нажать дважды
        foreach (var b in answerButtons)
            b.SetInteractable(false);

        OnAnswerReceived?.Invoke(answer);
        Close();
    }

    private void ClearQuestioning() {
        foreach (var button in answerButtons) {
            if (button == null) continue;
            button.Clicked -= HandleAnswerClicked;
            Destroy(button.gameObject);
        }
        answerButtons.Clear();

        if (questionField != null)
            questionField.text = string.Empty;
    }
}
