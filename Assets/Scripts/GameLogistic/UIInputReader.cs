using System;
using UnityEngine;
using UnityEngine.InputSystem;
using Zenject;

public class UIInputReader : IInitializable, IDisposable {
    private readonly InputService inputService;
    private InputSystem_Actions.UIActions uiActions;
    private Vector2 _navigationInput;
    public Action<Vector2> OnNavigationChanged;
    public Vector2 NavigationInput {
        set {
            if (_navigationInput == value)
                return;

            _navigationInput = value;
            OnNavigationChanged?.Invoke(value);
        }
        get => _navigationInput;
    }
    private Vector2 _angleInput;
    public Action<Vector2> OnAngleChanged;
    public Vector2 AngleInput {
        set {
            if (_angleInput == value)
                return;

            _angleInput = value;
            OnAngleChanged?.Invoke(value);
        }
        get => _angleInput;
    }
    public event Action OnUICancel;
    public event Action OnBrowseRequested;
    public event Action OnPrevRequested;
    public event Action OnNextRequested;
    public event Action OnOpen;
    public event Action OnClose;
    public event Action OnClick;
    public UIInputReader(InputService inputService) {
        this.inputService = inputService;
        uiActions = inputService.Actions.UI;
    }
    public void Initialize() {
        SubscribeToInputEvents();
    }
    public void Dispose() {
        UnsubscribeFromInputEvents();
    }

    private void UnsubscribeFromInputEvents() {
        uiActions.Cancel.performed -= HandleUICancelPerformed;
        uiActions.Submit.performed -= HanleSubmit;

        uiActions.Navigate.performed -= HandleNavigationPerformed;
        uiActions.Navigate.canceled -= HandleNavigationCanceled;
        uiActions.Click.canceled -= HandleClick;
    }

    private void SubscribeToInputEvents() {
        uiActions.Cancel.performed += HandleUICancelPerformed;
        uiActions.Submit.performed += HanleSubmit;

        uiActions.Navigate.performed += HandleNavigationPerformed;
        uiActions.Navigate.canceled += HandleNavigationCanceled;

        uiActions.Click.canceled += HandleClick;

        uiActions.Point.performed += HandlePointPerformed;
        uiActions.Point.canceled += HandlePointCanceled;
    }

    public Vector2 PointInput;

    private void HandlePointCanceled(InputAction.CallbackContext context) {
        PointInput = Vector2.zero;
    }

    private void HandlePointPerformed(InputAction.CallbackContext context) {
        PointInput = context.ReadValue<Vector2>();
    }

    private void HandleClick(InputAction.CallbackContext context) {
        OnClick?.Invoke();
    }

    
    private void HandleOpenCanceled(InputAction.CallbackContext context) {
        OnOpen?.Invoke();
    }

    private void HandleCloseCanceled(InputAction.CallbackContext context) {
        OnClose?.Invoke();
    }

    private void HandleUICancelPerformed(InputAction.CallbackContext context) => OnUICancel?.Invoke();
    private void HandleNavigationPerformed(InputAction.CallbackContext context) {
        var oldValue = NavigationInput;
        var newValue = context.ReadValue<Vector2>();

        NavigationInput = newValue;

        Debug.Log($"Navigation input: {NavigationInput}");
        if (NavigationInput.x > 0) {
            HandleNext(context);
        } else if (NavigationInput.x < 0) {
            HandlePrev(context);
        }
    }

    private void HandleNavigationCanceled(InputAction.CallbackContext context) {
        NavigationInput = Vector2.zero;
    }

    private void HandleAnglePerformed(InputAction.CallbackContext context) {
        AngleInput = context.ReadValue<Vector2>();
    }
    private void HandleAngleCanceled(InputAction.CallbackContext context) {
        AngleInput = Vector2.zero;
    }

    private void HandleNext(InputAction.CallbackContext ctx) {
        OnNextRequested?.Invoke();
    }

    private void HandlePrev(InputAction.CallbackContext ctx) {
        OnPrevRequested?.Invoke();
    }
    private void HanleSubmit(InputAction.CallbackContext context) {
        OnBrowseRequested?.Invoke();
    }
}