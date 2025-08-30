using System;
using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// Centralized input manager for the RTS game.
/// Uses Unity's Input System with a generated input actions class (RTSInputActions).
/// Provides static events so other systems can subscribe without direct references.
/// </summary>
public class InputManager : MonoBehaviour
{
    // --- Camera movement inputs ---
    public static event Action<Vector2> OnMoveInput;              // WASD/Arrow keys movement input
    public static event Action<float> OnZoomInput;                // Scroll wheel or pinch for zoom
    public static event Action<bool> OnRotateInput;               // Middle mouse button for rotation
    public static event Action<bool> OnBoostInput;                // Shift key for movement speed boost

    // --- Mouse position and actions ---
    public static event Action<Vector2> OnPointerPositionChanged; // Called whenever the pointer position changes
    public static event Action<Vector2> OnRightClick;             // Player right-clicked
    public static event Action<Vector2> OnLeftClick;              // Player left-clicked
    public static event Action OnLeftPress;                       // Left mouse button pressed down
    public static event Action OnLeftRelease;                     // Left mouse button released

    // --- Exit action (Escape key) ---
    public static event Action OnExitPressed;

    private RTSInputActions controls;
    private Vector2 lastPointerPosition;

    private void Awake()
    {
        controls = new RTSInputActions();

        // --- Movement ---
        controls.Camera.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        controls.Camera.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero);

        // --- Zoom ---
        controls.Camera.Zoom.performed += ctx => OnZoomInput?.Invoke(ctx.ReadValue<float>());
        controls.Camera.Zoom.canceled += ctx => OnZoomInput?.Invoke(0f);

        // --- Rotation ---
        controls.Camera.Rotate.performed += ctx => OnRotateInput?.Invoke(true);
        controls.Camera.Rotate.canceled += ctx => OnRotateInput?.Invoke(false);

        // --- Boost ---
        controls.Camera.Boost.performed += ctx => OnBoostInput?.Invoke(true);
        controls.Camera.Boost.canceled += ctx => OnBoostInput?.Invoke(false);

        // --- Pointer Position ---
        controls.Camera.PointerPosition.performed += ctx =>
        {
            lastPointerPosition = ctx.ReadValue<Vector2>();
            OnPointerPositionChanged?.Invoke(lastPointerPosition);
        };

        // --- Right Click ---
        controls.Camera.RightClick.performed += ctx => OnRightClick?.Invoke(lastPointerPosition);

        // --- Left Click / Press / Release ---
        controls.Camera.LeftClick.performed += ctx => OnLeftClick?.Invoke(lastPointerPosition);
        controls.Camera.LeftClick.started += ctx => OnLeftPress?.Invoke();
        controls.Camera.LeftClick.canceled += ctx => OnLeftRelease?.Invoke();

        // --- Exit (Escape) ---
        controls.Camera.Exit.performed += ctx => OnExitPressed?.Invoke();
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
