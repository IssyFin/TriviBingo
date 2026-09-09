using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "questions", menuName = "Game/Questions")]
public class QuestionData : ScriptableObject {
    public List<Question> Questions;
}

