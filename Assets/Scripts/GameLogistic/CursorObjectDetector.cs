using UnityEngine;
using Zenject;

public class CursorObjectDetector : MonoBehaviour, IObjectDetector {
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float detectionDistance = 100f; // Для луча с экрана дистанция обычно выше (3f может не хватить)

    private Camera _mainCamera;

    private Camera MainCamera {
        get {
            if (_mainCamera == null) _mainCamera = Camera.main;
            return _mainCamera;
        }
    }

    private UIInputReader input;
    [Inject]
    public void Construct(UIInputReader uIInputReader) {
        input = uIInputReader;
    }

    public bool TryDetectObject(out DetectionResult result) {
        Camera cam = MainCamera;

        if (cam == null) {
            result = default;
            return false;
        }

        Vector2 mousePosition = input.PointInput;

        // 2. Пускаем луч из точки на экране в мир
        Ray ray = cam.ScreenPointToRay(mousePosition);

        // 3. Выполняем Raycast
        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, targetLayer)) {
            result = new DetectionResult {
                GameObject = hit.collider.gameObject,
                Collider = hit.collider,
                HitPoint = hit.point,
                HitNormal = hit.normal
            };
            return true;
        }

        result = default;
        return false;
    }

    public GameObject DetectObject() {
        return TryDetectObject(out DetectionResult result) ? result.GameObject : null;
    }

    private void OnDrawGizmosSelected() {
        Camera cam = MainCamera;
        if (cam == null || input == null) return;

        Vector3 mousePosition = input.PointInput;
        Ray ray = cam.ScreenPointToRay(mousePosition);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(ray.origin, ray.direction * detectionDistance);
    }
}
