using UnityEngine;

/// <summary>
/// Controls an RTS-style camera, allowing smooth movement, zooming, and rotation.
/// Uses input events provided by the InputManager for modularity and flexibility.
/// </summary>
public class RTSCameraController : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 20f;         // Base movement speed for the camera
    [SerializeField] private float boostMultiplier = 2f;    // Speed multiplier when "boost" is active (e.g., Shift key)

    [Header("Zoom")]
    [SerializeField] private Transform cameraTransform;     // The child camera transform to adjust for zoom
    [SerializeField] private float zoomSpeed = 1f;          // How fast zoom changes based on input
    [SerializeField] private float zoomSmoothTime = 0.2f;   // Smooth damp time for zoom transitions
    [SerializeField] private Vector3 zoomClose = new Vector3(0f, 15f, -15f); // Local position for closest zoom
    [SerializeField] private Vector3 zoomFar = new Vector3(0f, 40f, -40f);   // Local position for farthest zoom

    [Header("Rotation")]
    [SerializeField] private float rotationSpeed = 5f;      // How fast the camera rotates based on mouse movement

    // Runtime input states
    private Vector2 moveInput;                              // WASD/Arrow keys input for panning
    private float zoomInput;                                // Scroll wheel or similar for zoom
    private bool isRotating;                                // Whether the rotation key/button is held down
    private bool isBoosting;                                // Whether the boost key/button is held down

    // Zoom state
    private float targetZoomFactor = 0.5f;                  // Interpolated zoom level (0 = close, 1 = far)
    private Vector3 currentZoomVelocity;                    // SmoothDamp helper velocity for zoom
    private Vector3 lastPosition;                           // Last recorded position (currently unused, could be for edge scrolling detection)

    #region Event Subscription
    private void OnEnable()
    {
        // Subscribe to input events from the InputManager
        InputManager.OnMoveInput += OnMove;
        InputManager.OnZoomInput += OnZoom;
        InputManager.OnRotateInput += OnRotate;
        InputManager.OnBoostInput += OnBoost;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
        InputManager.OnMoveInput -= OnMove;
        InputManager.OnZoomInput -= OnZoom;
        InputManager.OnRotateInput -= OnRotate;
        InputManager.OnBoostInput -= OnBoost;
    }
    #endregion

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    #region Input Callbacks
    private void OnMove(Vector2 input) => moveInput = input;
    private void OnZoom(float input) => zoomInput = input;
    private void OnRotate(bool isPressed) => isRotating = isPressed;
    private void OnBoost(bool isPressed) => isBoosting = isPressed;
    #endregion

    /// <summary>
    /// Handles horizontal camera movement based on player input.
    /// Uses Space.Self so that forward/back movement is relative to the camera's rotation.
    /// </summary>
    private void HandleMovement()
    {
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);
        float finalSpeed = moveSpeed * (isBoosting ? boostMultiplier : 1f);
        Vector3 movement = direction * finalSpeed * Time.deltaTime;

        if (movement != Vector3.zero)
        {
            transform.Translate(movement, Space.Self);
            lastPosition = transform.position; // Currently not used, but could be useful for detecting inactivity
        }
    }

    /// <summary>
    /// Handles zooming in and out smoothly between the predefined close and far positions.
    /// </summary>
    private void HandleZoom()
    {
        targetZoomFactor -= zoomInput * zoomSpeed * 0.1f; // Adjust zoom target based on input
        targetZoomFactor = Mathf.Clamp01(targetZoomFactor); // Clamp between 0 (close) and 1 (far)

        Vector3 targetPosition = Vector3.Lerp(zoomClose, zoomFar, targetZoomFactor);
        cameraTransform.localPosition = Vector3.SmoothDamp(
            cameraTransform.localPosition,
            targetPosition,
            ref currentZoomVelocity,
            zoomSmoothTime
        );
    }

    /// <summary>
    /// Handles camera rotation around the Y-axis when the rotation input is active.
    /// Rotation speed is scaled by delta time for frame independence.
    /// </summary>
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
