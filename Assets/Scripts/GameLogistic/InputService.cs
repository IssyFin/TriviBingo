using System;
using UnityEngine;

public class InputService : IDisposable {
    public InputSystem_Actions Actions { get; }

    public InputService() {
        Actions = new InputSystem_Actions();
        Actions.Enable();
    }

    public void SwitchToGameplay() {
        DisableCursor();
        Actions.UI.Disable();
        Actions.Player.Enable();
    }

    public void SwitchToUI() {
        EnableCursor();
        Actions.Player.Disable();
        Actions.UI.Enable();
    }

    public void Dispose() {
        Actions?.Dispose();
    }

    internal void EnableCursor() {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    internal void DisableCursor() {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }
}