using UnityEngine;

using UnityEngine;

public class StaticCameraDetector : BaseObjectDetector {
    [Header("Static Detector Settings")]
    [SerializeField] private float raycastRadius = 0.2f;

    private Transform OriginTransform => MainCamera != null ? MainCamera.transform : transform;

    public override bool TryDetectObject(out DetectionResult result) {
        Transform origin = OriginTransform;

        if (Physics.SphereCast(origin.position, raycastRadius, origin.forward, out RaycastHit hit, detectionDistance, targetLayer)) {

            // Защита от старта каста внутри коллайдера
            Vector3 hitPoint = hit.point == Vector3.zero
                ? hit.collider.ClosestPoint(origin.position)
                : hit.point;

            result = new DetectionResult(hit.collider.gameObject, hit.collider, hitPoint, hit.normal);
            return true;
        }

        result = default;
        return false;
    }

    private void OnDrawGizmosSelected() {
        Transform origin = OriginTransform;
        if (origin == null) return;

        Gizmos.color = Color.green;
        Gizmos.DrawRay(origin.position, origin.forward * detectionDistance);

        Gizmos.color = new Color(0, 1, 0, 0.5f);
        Vector3 endPoint = origin.position + origin.forward * detectionDistance;
        Gizmos.DrawWireSphere(origin.position, raycastRadius);
        Gizmos.DrawWireSphere(endPoint, raycastRadius);
    }
}
