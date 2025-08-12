using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Main handler for selecting units and issuing commands in RTS style.
/// Supports both click and drag selection, and right-click commands for movement and resource gathering.
/// </summary>
public class UnitSelectionHandler : MonoBehaviour
{
    // Event fired when player right-clicks on the ground, passes the target position
    public event Action<Vector3> OnGroundClick;

    [Header("UI References")]
    [SerializeField] private RectTransform selectionBoxUI; // Visual rectangle for drag selection box
    [SerializeField] private Canvas canvas;                // Canvas for UI elements

    [Header("Layers")]
    [SerializeField] private LayerMask unitLayer;          // Layer for selectable units
    [SerializeField] private LayerMask groundLayer;        // Layer for ground clicks
    [SerializeField] private LayerMask resourceLayer;      // Layer for resource nodes

    [Header("Camera & Input")]
    [SerializeField] private Camera mainCamera;            // Camera for raycasting
    [SerializeField] private float dragThreshold = 10f;    // Minimum drag distance before selection box appears

    [SerializeField] private float gatherClickRadius = 2f; // Radius around click to detect nearby resources

    private Vector2 startPosition;                          // Mouse position at drag start
    private Vector2 currentMousePosition;                   // Current mouse position
    private bool isDragging = false;                        // Are we currently dragging selection box

    // List of currently selected units
    private List<SelectableUnit> selectedUnits = new();

    // Event fired when selection changes, passes count of selected units
    public event Action<int> OnSelectionChanged;

    private void Start()
    {
        // Initially hide selection box UI and clear any selections
        selectionBoxUI.gameObject.SetActive(false);
        DeselectAll();
    }

    private void OnEnable()
    {
        // Subscribe to input events from a centralized InputManager (decouples input logic)
        InputManager.OnPointerPositionChanged += OnPointerPositionChanged;
        InputManager.OnRightClick += HandleRightClick;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or unintended callbacks
        InputManager.OnPointerPositionChanged -= OnPointerPositionChanged;
        InputManager.OnRightClick -= HandleRightClick;
    }

    // Update current mouse position every frame from InputManager
    private void OnPointerPositionChanged(Vector2 pointerPos)
    {
        currentMousePosition = pointerPos;
    }

    private void Update()
    {
        // Handle left mouse button down: start tracking for selection
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = currentMousePosition;
            isDragging = false;
        }
        // While holding left mouse button: check drag distance and show selection box if needed
        else if (UnityEngine.InputSystem.Mouse.current.leftButton.isPressed)
        {
            if (!isDragging && Vector2.Distance(currentMousePosition, startPosition) > dragThreshold)
            {
                isDragging = true;
                selectionBoxUI.gameObject.SetActive(true);
            }

            if (isDragging)
            {
                UpdateSelectionBoxUI();
            }
        }
        // On left mouse button release: finalize selection (box or single click)
        else if (UnityEngine.InputSystem.Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
            {
                SelectUnitsInBox();
            }
            else
            {
                TrySelectSingleUnitOrDeselect();
            }

            isDragging = false;
            selectionBoxUI.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Updates the position and size of the UI selection box based on drag start and current mouse.
    /// </summary>
    private void UpdateSelectionBoxUI()
    {
        // Convert screen positions to local canvas points to size/position selection box correctly
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            startPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 startLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            currentMousePosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 currentLocal);

        Vector2 size = currentLocal - startLocal;
        selectionBoxUI.anchoredPosition = startLocal + size / 2f;
        selectionBoxUI.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    /// <summary>
    /// Selects all units whose screen positions are inside the current selection box.
    /// </summary>
    private void SelectUnitsInBox()
    {
        DeselectAll();

        // Construct Rect representing selection box area
        Rect selectionRect = new Rect(
            selectionBoxUI.anchoredPosition - selectionBoxUI.sizeDelta / 2f,
            selectionBoxUI.sizeDelta);

        // Iterate all selectable units in scene and check if inside selection box
        foreach (SelectableUnit unit in GameObject.FindObjectsByType<SelectableUnit>(UnityEngine.FindObjectsSortMode.None))
        {
            Vector2 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);

            // Convert screen position to local canvas space for comparison
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint);

            if (selectionRect.Contains(localPoint))
            {
                unit.Select();
                selectedUnits.Add(unit);
            }
        }

        OnSelectionChanged?.Invoke(selectedUnits.Count);
    }

    /// <summary>
    /// Attempts to select a single unit by raycasting at mouse position,
    /// or deselect all if clicking empty ground.
    /// </summary>
    private void TrySelectSingleUnitOrDeselect()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // Raycast for selectable unit
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, unitLayer))
        {
            if (hit.collider.TryGetComponent(out SelectableUnit unit))
            {
                DeselectAll();
                unit.Select();
                selectedUnits.Add(unit);
                OnSelectionChanged?.Invoke(selectedUnits.Count);
                return;
            }
        }

        // Raycast for ground click to deselect all units
        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundLayer))
        {
            DeselectAll();
            OnSelectionChanged?.Invoke(selectedUnits.Count);
        }
    }

    /// <summary>
    /// Deselects all currently selected units and clears selection list.
    /// </summary>
    private void DeselectAll()
    {
        foreach (SelectableUnit unit in selectedUnits)
        {
            if (unit != null)
            {
                unit.Deselect();
            }
        }
        selectedUnits.Clear();
    }

    /// <summary>
    /// Handles issuing commands on right-click:
    /// - If resources nearby click: start gathering
    /// - Else: move units to clicked point
    /// </summary>
    public void HandleRightClick(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Raycast to ground to get target point
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Check for nearby resources within gatherClickRadius of clicked point
            Collider[] hits = Physics.OverlapSphere(hit.point, gatherClickRadius, resourceLayer);

            if (hits.Length > 0)
            {
                // Pick first resource node nearby (could be improved to nearest)
                ResourceNode resourceNode = hits[0].GetComponent<ResourceNode>();

                if (resourceNode != null)
                {
                    // For each selected unit, try to start gathering resource or move to it if gathering not supported
                    foreach (var unit in selectedUnits)
                    {
                        if (unit.TryGetComponent<ResourceGathering>(out var gather))
                        {
                            gather.StartGathering(resourceNode);
                        }
                        else if (unit.TryGetComponent<UnitMovement>(out var mover))
                        {
                            mover.MoveTo(resourceNode.transform.position);
                        }
                    }
                    return; // Command handled, exit early
                }
            }
            else
            {
                // No resource nearby, send standard move command
                SendMoveCommand(hit.point);
            }
        }
    }

    /// <summary>
    /// Sends move commands to all selected units, spreading them around target position to avoid clustering.
    /// </summary>
    private void SendMoveCommand(Vector3 point)
    {
        selectedUnits.RemoveAll(u => u == null); // Clean null references

        int unitCount = selectedUnits.Count;
        float radius = 1.5f; // Radius for spread formation

        for (int i = 0; i < unitCount; i++)
        {
            Vector3 targetPosition;

            if (unitCount == 1)
            {
                targetPosition = point;
            }
            else
            {
                // Spread units evenly in circle around point to avoid stacking
                float angle = i * Mathf.PI * 2f / unitCount;
                float offsetX = Mathf.Cos(angle) * radius;
                float offsetZ = Mathf.Sin(angle) * radius;

                targetPosition = new Vector3(point.x + offsetX, point.y, point.z + offsetZ);
            }

            // Priority: if unit supports resource gathering movement, use it to cancel gather if needed
            if (selectedUnits[i].TryGetComponent<ResourceGathering>(out var gather))
            {
                gather.MoveTo(targetPosition);
            }
            else if (selectedUnits[i].TryGetComponent<UnitMovement>(out var mover))
            {
                mover.MoveTo(targetPosition);
            }
        }

        OnGroundClick?.Invoke(point);
    }
}
