using UnityEngine;
using Zenject;

public class CursorObjectDetector : BaseObjectDetector {

    private UIInputReader _input;

    [Inject]
    public void Construct(UIInputReader uIInputReader) {
        _input = uIInputReader;
    }

    public override bool TryDetectObject(out DetectionResult result) {
        Camera cam = MainCamera;

        if (cam == null || _input == null) {
            result = default;
            return false;
        }

        Ray ray = cam.ScreenPointToRay(_input.PointInput);

        if (Physics.Raycast(ray, out RaycastHit hit, detectionDistance, targetLayer)) {
            result = new DetectionResult(hit.collider.gameObject, hit.collider, hit.point, hit.normal);
            return true;
        }

        result = default;
        return false;
    }

    private void OnDrawGizmosSelected() {
        // Защита: в Edit Mode _input не проинжекчен, поэтому выходим
        if (!Application.isPlaying || _input == null) return;

        Camera cam = MainCamera;
        if (cam == null) return;

        Vector3 mousePosition = _input.PointInput;
        Ray ray = cam.ScreenPointToRay(mousePosition);

        Gizmos.color = Color.green;
        Gizmos.DrawRay(ray.origin, ray.direction * detectionDistance);
    }
}
