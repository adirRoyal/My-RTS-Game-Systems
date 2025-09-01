using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Controls a top-down RTS-style camera:
/// - Move with WASD / arrow keys
/// - Zoom in/out with mouse wheel
/// - Rotate camera with mouse drag
/// - Boost movement speed with a key (Shift)
/// Uses Unity's new Input System via InputManager events.
/// </summary>
public class RTSCameraController : MonoBehaviour
{
    [Header("Movement Settings")]
    [SerializeField] private float moveSpeed = 20f;       // normal camera speed
    [SerializeField] private float boostMultiplier = 2f;  // speed multiplier when boosting

    [Header("Zoom Settings")]
    [SerializeField] private Transform cameraTransform;   // the actual camera object
    [SerializeField] private float zoomSpeed = 1f;        // how fast zoom changes
    [SerializeField] private float zoomSmoothTime = 0.2f; // smoothing for zoom movement
    [SerializeField] private Vector3 zoomClose = new Vector3(0f, 15f, -15f); // closest zoom pos
    [SerializeField] private Vector3 zoomFar = new Vector3(0f, 40f, -40f);   // farthest zoom pos

    [Header("Rotation Settings")]
    [SerializeField] private float rotationSpeed = 5f; // speed of rotation when dragging

    // --- Input variables ---
    private Vector2 moveInput;       // WASD / arrow keys
    private float zoomInput;         // mouse scroll
    private bool isRotating;         // true when holding rotate button
    private bool isBoosting;         // true when holding boost key

    // --- Zoom smoothing helpers ---
    private float targetZoomFactor = 0.5f;      // 0 = zoomClose, 1 = zoomFar
    private Vector3 currentZoomVelocity;        // helper for SmoothDamp

    #region Input Callbacks
    private void OnEnable()
    {
        // Subscribe to input events
        InputManager.OnMoveInput += OnMove;
        InputManager.OnZoomInput += OnZoom;
        InputManager.OnRotateInput += OnRotate;
        InputManager.OnBoostInput += OnBoost;
    }

    private void OnDisable()
    {
        // Unsubscribe from input events
        InputManager.OnMoveInput -= OnMove;
        InputManager.OnZoomInput -= OnZoom;
        InputManager.OnRotateInput -= OnRotate;
        InputManager.OnBoostInput -= OnBoost;
    }

    // these methods are called from InputManager
    private void OnMove(Vector2 input) => moveInput = input;
    private void OnZoom(float input) => zoomInput = input;
    private void OnRotate(bool pressed) => isRotating = pressed;
    private void OnBoost(bool pressed) => isBoosting = pressed;
    #endregion

    private void Update()
    {
        HandleMovement();
        HandleZoom();
        HandleRotation();
    }

    /// <summary>
    /// Moves the camera based on input and boost.
    /// </summary>
    private void HandleMovement()
    {
        // convert input to world movement
        Vector3 direction = new Vector3(moveInput.x, 0f, moveInput.y);

        // apply boost if needed
        float finalSpeed = moveSpeed * (isBoosting ? boostMultiplier : 1f);

        // move camera
        transform.Translate(direction * finalSpeed * Time.deltaTime, Space.Self);
    }

    /// <summary>
    /// Zoom camera in/out smoothly.
    /// </summary>
    private void HandleZoom()
    {
        // adjust zoom factor based on input
        targetZoomFactor -= zoomInput * zoomSpeed * 0.1f;
        targetZoomFactor = Mathf.Clamp01(targetZoomFactor); // keep between 0 and 1

        // interpolate between close and far positions
        Vector3 targetPosition = Vector3.Lerp(zoomClose, zoomFar, targetZoomFactor);

        // smooth movement
        cameraTransform.localPosition = Vector3.SmoothDamp(
            cameraTransform.localPosition,
            targetPosition,
            ref currentZoomVelocity,
            zoomSmoothTime
        );
    }

    /// <summary>
    /// Rotate camera horizontally when rotating is enabled.
    /// </summary>
    private void HandleRotation()
    {
        if (isRotating && Mouse.current != null)
        {
            // rotate around world Y-axis based on mouse delta
            float rotationDelta = Mouse.current.delta.ReadValue().x;
            transform.Rotate(Vector3.up, rotationDelta * rotationSpeed * Time.deltaTime, Space.World);
        }
    }
}
