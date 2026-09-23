using System;
using UnityEngine;

public interface IInteractionService {
    event Action<GameObject> Selected;
    event Action<GameObject, bool> HoverChanged;
}

public interface ITileTarget {
    Tile Tile { get; }
}

public interface IInteractable {
    void OnSelect(bool isSelected);
    void OnHoverEnter();
    void OnHoverExit();
}