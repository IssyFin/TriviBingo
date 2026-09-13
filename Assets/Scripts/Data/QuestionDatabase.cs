using System;
using System.Collections.Generic;
using UnityEngine;
using Sirenix.OdinInspector;
using Sirenix.Serialization;

[CreateAssetMenu(fileName = "QuestionDatabase", menuName = "Game/Question Database")]
public class QuestionDatabase : SerializedScriptableObject {
    [TableList]
    [ListDrawerSettings(ShowFoldout = true)]
    public List<QuestionData> Questions = new();

    [OnInspectorInit]
    private void EnsureIds() {
#if UNITY_EDITOR
        foreach (var q in Questions)
            if (string.IsNullOrEmpty(q.Id))
                q.Id = System.Guid.NewGuid().ToString("N");
#endif
    }
}

public enum QuestionCategory {
    Any,
    History,
    Science,
    Geography,
    Movies,
    PopCulture
}

[Serializable]
public class QuestionData {
    [HorizontalGroup("Header")]
    [ReadOnly]
    [HideInInspector]
    public string Id = string.Empty;

    [TextArea(2, 5)]
    public string Text = string.Empty;

    [EnumToggleButtons]
    public QuestionCategory Category;

    [ListDrawerSettings(ShowFoldout = true)]
    public List<Answer> Answers = new();
}

[Serializable]
public class Answer {
    [TableColumnWidth(300)]
    public string Text = string.Empty;

    public bool IsCorrect;
}
