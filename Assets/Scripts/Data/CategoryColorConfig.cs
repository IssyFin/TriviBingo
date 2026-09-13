using Sirenix.OdinInspector;
using System.Collections.Generic;
using UnityEngine;


[CreateAssetMenu(fileName = "CategoryColorConfig", menuName = "Game/Configs/Category Color Config")]
public class CategoryColorConfig : SerializedScriptableObject {
    [SerializeField]
    public Dictionary<QuestionCategory, Color> CategoryColors = new();

    [SerializeField]
    public Color DefaultColor = Color.white;
}