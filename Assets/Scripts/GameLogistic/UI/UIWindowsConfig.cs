using System;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "UI Windows Config", menuName = "UI/Windows Config")]
public class UIWindowsConfig : ScriptableObject {
    [SerializeField] private List<MonoBehaviour> windowPrefabs; // каждый должен иметь компонент IWindow

    public Dictionary<Type, GameObject> BuildMap() {
        var map = new Dictionary<Type, GameObject>();
        foreach (var prefab in windowPrefabs) {
            if (prefab is IWindow)
                map[prefab.GetType()] = prefab.gameObject;
            else
                Debug.LogError($"{prefab.name} does not implement IWindow");
        }
        return map;
    }
}
