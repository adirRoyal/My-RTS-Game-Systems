using UnityEngine;

public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;
    [SerializeField] private float boostMultiplier = 2f;

    [Header("Zoom")]
    [SerializeField] private Transform cameraTransform;
    [SerializeField] private float zoomSpeed = 1f;
    [SerializeField] private float zoomSmoothTime = 0.2f;
    [SerializeField] private Vector3 zoomClose = new Vector3(0f, 15f, -15f);
    [SerializeField] private Vector3 zoomFar = new Vector3(0f, 40f, -40f);

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;

    private Vector2 moveInput;
    private float zoomInput;
    private bool isRotating;
    private bool isBoosting;

    private float targetZoomFactor = 0.5f;
    private Vector3 currentZoomVelocity;
    private Vector3 lastPosition;

    private void OnEnable()
    {
        InputManager.OnMoveInput += OnMove;
        InputManager.OnZoomInput += OnZoom;
        InputManager.OnRotateInput += OnRotate;
        InputManager.OnBoostInput += OnBoost;
    }

    private void OnDisable()
    {
        InputManager.OnMoveInput -= OnMove;
        InputManager.OnZoomInput -= OnZoom;
        InputManager.OnRotateInput -= OnRotate;
        InputManager.OnBoostInput -= OnBoost;
    }

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    private void OnMove(Vector2 input) => moveInput = input;
    private void OnZoom(float input) => zoomInput = input;
    private void OnRotate(bool isPressed) => isRotating = isPressed;
    private void OnBoost(bool isPressed) => isBoosting = isPressed;

    private void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        float finalSpeed = moveSpeed * (isBoosting ? boostMultiplier : 1f);
        Vector3 movement = direction * finalSpeed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            transform.Translate(movement, Space.Self);
            lastPosition = transform.position;
        }
    }


    private void HandleZoom()
    {
        targetZoomFactor -= zoomInput * zoomSpeed * 0.1f;
        targetZoomFactor = Mathf.Clamp01(targetZoomFactor);

        Vector3 targetPosition = Vector3.Lerp(zoomClose, zoomFar, targetZoomFactor);
        cameraTransform.localPosition = Vector3.SmoothDamp(cameraTransform.localPosition, targetPosition, ref currentZoomVelocity, zoomSmoothTime);
    }

    private void HandleRotation()
    {
        if (isRotating && UnityEngine.InputSystem.Mouse.current != null)
        {
            float rotationDelta = UnityEngine.InputSystem.Mouse.current.delta.ReadValue().x;
            if (rotationDelta != 0)
                transform.Rotate(Vector3.up, rotationDelta * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
