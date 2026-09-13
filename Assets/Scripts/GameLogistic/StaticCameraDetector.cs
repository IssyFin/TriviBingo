using UnityEngine;

public struct DetectionResult {
    public GameObject GameObject;
    public Collider Collider;
    public Vector3 HitPoint;
    public Vector3 HitNormal;

    public bool IsValid => GameObject != null;
}

public class StaticCameraDetector : MonoBehaviour, IObjectDetector {
    [Header("Detection Settings")]
    [SerializeField] private LayerMask targetLayer;
    [SerializeField] private float detectionDistance = 3f;
    [SerializeField] private float raycastRadius = 0.2f;

    private Camera _mainCamera;

    private Transform OriginTransform {
        get {
            if (_mainCamera == null) _mainCamera = Camera.main;
            return _mainCamera != null ? _mainCamera.transform : transform;
        }
    }

    public bool TryDetectObject(out DetectionResult result) {
        Transform origin = OriginTransform;

        if (Physics.SphereCast(
            origin.position,
            raycastRadius,
            origin.forward,
            out RaycastHit hit,
            detectionDistance,
            targetLayer)) {

            Vector3 hitPoint = hit.point;

            if (hitPoint == Vector3.zero) {
                hitPoint = hit.collider.ClosestPoint(origin.position);
            }

            result = new DetectionResult {
                GameObject = hit.collider.gameObject,
                Collider = hit.collider,
                HitPoint = hitPoint,
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
        Transform origin = OriginTransform;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin.position, origin.forward * detectionDistance);

        Gizmos.color = Color.green * 0.5f;
        Vector3 endPoint = origin.position + origin.forward * detectionDistance;
        Gizmos.DrawWireSphere(origin.position, raycastRadius);
        Gizmos.DrawWireSphere(endPoint, raycastRadius);
    }
}
