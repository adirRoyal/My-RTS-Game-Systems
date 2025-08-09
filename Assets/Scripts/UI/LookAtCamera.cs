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

        // ���� �� ���forward �� �������� ���� ���� ��direction ������
        Vector3 direction = transform.position - mainCamera.transform.position;
        transform.rotation = Quaternion.LookRotation(direction);
    }
}
