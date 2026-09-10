using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "questions", menuName = "Game/Questions")]
public class QuestionDatabase : ScriptableObject {
    public List<QuestionData> Questions;
}

public enum QuestionTheme {
    Any,
    History,
    Science,
    Geography,
    Movies,
    PopCulture
}

[Serializable]
public class QuestionData {
    public string Id = string.Empty;
    public string Text = string.Empty;
    public List<Answer> Answers = new();
    public QuestionTheme Theme = new();
}

[Serializable]
public class Answer {
    public string Text = string.Empty;
    public bool IsCorrect;
}
