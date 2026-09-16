using System;
using UnityEngine;

public interface IUIElement {
    void Open();
    void Close();
}

public interface IWindow : IUIElement {
    event Action<IWindow> OnClosed;
}

public class UIElementBase : MonoBehaviour, IUIElement {
    public virtual void Open() {
        if (gameObject.activeSelf) return;
        gameObject.SetActive(true);
    }

    public virtual void Close() {
        if (!gameObject.activeSelf) return;
        gameObject.SetActive(false);
    }

    public void Toggle(bool isEnabled) {
        if (isEnabled) {
            Open();
        } else {
            Close();
        }
    }
}

public class UIWindow : UIElementBase, IWindow {
    public event Action<IWindow> OnClosed;

    public override void Close() {
        if (!gameObject.activeSelf) return;
        base.Close();
        OnClosed?.Invoke(this);
    }
}