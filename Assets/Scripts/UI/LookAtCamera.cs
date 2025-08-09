using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;

    private void Awake()
    {
        mainCamera = Camera.main;
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;

        // סובב כך שה־forward של האובייקט יהיה הפוך ל־direction למצלמה
        Vector3 direction = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
