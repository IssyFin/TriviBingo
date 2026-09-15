using UnityEngine;

public interface IObjectDetector {
    GameObject DetectObject();
    bool TryDetectObject(out DetectionResult result);
}