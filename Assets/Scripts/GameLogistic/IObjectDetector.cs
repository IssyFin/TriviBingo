using UnityEngine;
using Zenject;

public interface IObjectDetector {
    GameObject DetectObject();
    bool TryDetectObject(out DetectionResult result);
}

public abstract class BaseObjectDetector : MonoBehaviour, IObjectDetector {
    [Header("Base Detection Settings")]
    [SerializeField] protected LayerMask targetLayer;
    [SerializeField] protected float detectionDistance = 100f;

    protected Camera mainCamera;

    public void Awake() {
        mainCamera = Camera.main;
    }

    // Фоллбэк на случай, если скрипт тестируется на сцене без Zenject
    protected Camera MainCamera {
        get {
            if (mainCamera == null) mainCamera = Camera.main;
            return mainCamera;
        }
    }

    public abstract bool TryDetectObject(out DetectionResult result);

    // Эта логика одинакова для всех детекторов, реализуем ее один раз здесь
    public GameObject DetectObject() {
        return TryDetectObject(out DetectionResult result) ? result.GameObject : null;
    }
}

// readonly структура работает быстрее и защищает от случайного изменения полей
public readonly struct DetectionResult {
    public GameObject GameObject { get; }
    public Collider Collider { get; }
    public Vector3 HitPoint { get; }
    public Vector3 HitNormal { get; }

    public bool IsValid => GameObject != null;

    public DetectionResult(GameObject gameObject, Collider collider, Vector3 hitPoint, Vector3 hitNormal) {
        GameObject = gameObject;
        Collider = collider;
        HitPoint = hitPoint;
        HitNormal = hitNormal;
    }
}