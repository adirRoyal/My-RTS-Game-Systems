using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
    private Camera mainCamera;  // Reference to main camera

    private void Awake()
    {
        mainCamera = Camera.main;  // Get the main camera on Awake
    }

    private void LateUpdate()
    {
        if (mainCamera == null) return;  // Do nothing if no camera found

        // Calculate direction from object to camera
        Vector3 direction = transform.position - mainCamera.transform.position;

        // Rotate object to look toward camera (object's forward faces camera)
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
