using System;
using System.Collections.Generic;
using UnityEngine;
using Zenject;

// TElement — "семейство" элементов (например IWindow или IUIElement).
// TConcrete — конкретный тип внутри семейства (например InventoryWindow).
// Параметр метода намеренно называется иначе, чем параметр интерфейса,
// чтобы не затенять его и не терять ограничение (constraint).
public interface IUIService<TElement> where TElement : class, IUIElement {
    TConcrete Show<TConcrete>() where TConcrete : Component, TElement;
    void Hide<TConcrete>() where TConcrete : Component, TElement;
    void Destroy<TConcrete>() where TConcrete : Component, TElement;
}

public interface IWindowService {
    T OpenWindow<T>() where T : Component, IWindow;
    void CloseTopWindow();
}

public interface IHudService : IUIService<IUIElement> {
}

public abstract class UIBaseService<TElement> : IUIService<TElement> where TElement : class, IUIElement {
    protected readonly DiContainer Container;
    protected readonly UIRoot UIRoot;
    protected readonly Dictionary<Type, GameObject> PrefabMap;
    protected readonly Dictionary<Type, TElement> ActiveElements = new();

    protected UIBaseService(
        DiContainer container,
        UIRoot uiRoot,
        Dictionary<Type, GameObject> prefabMap) {
        Container = container;
        UIRoot = uiRoot;
        PrefabMap = prefabMap;
    }

    public virtual TConcrete Show<TConcrete>() where TConcrete : Component, TElement {
        var type = typeof(TConcrete);

        // Если уже существует - просто открываем
        if (ActiveElements.TryGetValue(type, out var existing)) {
            existing.Open();
            return existing as TConcrete;
        }

        // Создаем новый
        if (!PrefabMap.TryGetValue(type, out var prefab)) {
            Debug.LogError($"No prefab registered for {type}");
            return null;
        }

        var instance = InstantiateElement<TConcrete>(prefab);
        ActiveElements[type] = instance;
        instance.Open();

        return instance;
    }

    public virtual void Hide<TConcrete>() where TConcrete : Component, TElement {
        if (ActiveElements.TryGetValue(typeof(TConcrete), out var element)) {
            element.Close();
        }
    }

    public virtual void Destroy<TConcrete>() where TConcrete : Component, TElement {
        if (ActiveElements.TryGetValue(typeof(TConcrete), out var element)) {
            ActiveElements.Remove(typeof(TConcrete));
            if (element is MonoBehaviour mono) {
                GameObject.Destroy(mono.gameObject);
            }
        }
    }

    protected virtual TConcrete InstantiateElement<TConcrete>(GameObject prefab) where TConcrete : Component, TElement {
        return Container.InstantiatePrefabForComponent<TConcrete>(prefab, GetContainer());
    }

    protected abstract Transform GetContainer();
}

public class UIWindowService : UIBaseService<IWindow>, IWindowService, IInitializable, IDisposable {
    private readonly InputService _inputService;
    private readonly UIInputReader _uiInputReader;
    private readonly List<IWindow> _openWindows = new();

    public UIWindowService(
        InputService input,
        DiContainer container,
        UIInputReader uiInputReader,
        UIWindowsConfig config,
        UIRoot uiRoot)
        : base(container, uiRoot, config.BuildMap()) {
        _inputService = input;
        _uiInputReader = uiInputReader;
    }

    protected override Transform GetContainer() => UIRoot.WindowsContainer;

    public void Initialize() => _uiInputReader.OnUICancel += CloseTopWindow;
    public void Dispose() => _uiInputReader.OnUICancel -= CloseTopWindow;

    public T OpenWindow<T>() where T : Component, IWindow {
        var window = Show<T>();
        if (window == null) return null;

        // Если окно уже открыто, перемещаем его в конец списка (делаем самым "верхним")
        if (_openWindows.Contains(window)) {
            _openWindows.Remove(window);
        } else {
            window.OnClosed += HandleWindowClosed;
        }

        _openWindows.Add(window);
        _inputService.SwitchToUI();

        return window;
    }

    public void CloseTopWindow() {
        if (_openWindows.Count > 0) {
            // Закрываем самое верхнее (последнее добавленное) окно
            _openWindows[_openWindows.Count - 1].Close();
        }
    }

    private void HandleWindowClosed(IWindow window) {
        window.OnClosed -= HandleWindowClosed;

        // Безопасно удаляем окно из любой позиции списка
        _openWindows.Remove(window);

        // Переключаем инпут обратно на геймплей только если открытых окон вообще не осталось
        if (_openWindows.Count == 0) {
            _inputService.SwitchToGameplay();
        }
    }
}

public class HudService : UIBaseService<IUIElement>, IHudService {
    public HudService(
        DiContainer container,
        UIHudConfig config,
        UIRoot uiRoot)
        : base(container, uiRoot, config.BuildHudMap()) {
    }


    protected override Transform GetContainer() => UIRoot.HudContainer;
}
