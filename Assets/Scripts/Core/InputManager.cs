using System;
using UnityEngine;
using UnityEngine.InputSystem;

public class InputManager : MonoBehaviour
{
    public static event Action<Vector2> OnMoveInput;
    public static event Action<float> OnZoomInput;
    public static event Action<bool> OnRotateInput;
    public static event Action<bool> OnBoostInput;
    public static event Action<Vector2> OnPointerPositionChanged;
    public static event Action<Vector2> OnRightClick;

    private RTSInputActions controls;
    private Vector2 lastPointerPosition;

    private void Awake()
    {
        controls = new RTSInputActions();

        controls.Camera.Move.performed += ctx => OnMoveInput?.Invoke(ctx.ReadValue<Vector2>());
        controls.Camera.Move.canceled += ctx => OnMoveInput?.Invoke(Vector2.zero);

        controls.Camera.Zoom.performed += ctx => OnZoomInput?.Invoke(ctx.ReadValue<float>());
        controls.Camera.Zoom.canceled += ctx => OnZoomInput?.Invoke(0f);

        controls.Camera.Rotate.performed += ctx => OnRotateInput?.Invoke(true);
        controls.Camera.Rotate.canceled += ctx => OnRotateInput?.Invoke(false);

        controls.Camera.Boost.performed += ctx => OnBoostInput?.Invoke(true);
        controls.Camera.Boost.canceled += ctx => OnBoostInput?.Invoke(false);

        controls.Camera.PointerPosition.performed += ctx =>
        {
            lastPointerPosition = ctx.ReadValue<Vector2>();
            OnPointerPositionChanged?.Invoke(lastPointerPosition);
        };

        controls.Camera.RightClick.performed += ctx =>
        {
            OnRightClick?.Invoke(lastPointerPosition);
        };
    }

    private void OnEnable() => controls.Enable();
    private void OnDisable() => controls.Disable();
}
