using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UI Hud Config", menuName = "UI/Hud Config")]
public class UIHudConfig : ScriptableObject {
    [SerializeField] private List<MonoBehaviour> prefabs;

    public Dictionary<Type, GameObject> BuildHudMap() {
        var map = new Dictionary<Type, GameObject>();
        foreach (var prefab in prefabs) {
            if (prefab is IUIElement)
                map[prefab.GetType()] = prefab.gameObject;
            else
                Debug.LogError($"{prefab.name} does not implement IWindow");
        }
        return map;
    }
}