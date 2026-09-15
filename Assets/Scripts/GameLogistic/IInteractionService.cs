using System;
using UnityEngine;

public interface IInteractionService {
    event Action<GameObject> Selected;
    event Action<GameObject, bool> HoverChanged;
}

public interface IInteractable {
    void OnSelect();
    void OnHoverEnter();
    void OnHoverExit();
}
