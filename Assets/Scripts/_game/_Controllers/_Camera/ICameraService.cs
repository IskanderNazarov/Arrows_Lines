using UnityEngine;

public interface ICameraService {
    void ShakeCamera(float duration, float strength);
    void ResetCamera();
}