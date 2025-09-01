using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles unit and building selection (click & drag box) and issues commands via right-click.
/// Integrates with UI (selection panel) and resource gathering logic.
/// </summary>
public class UnitSelectionHandler : MonoBehaviour
{
    public int SelectedCount => selectedObjects.Count; // Public count of currently selected objects

    [Header("UI References")]
    [SerializeField] private RectTransform selectionBoxUI; // UI box for drag selection
    [SerializeField] private Canvas canvas;               // Canvas used to draw selection box

    [Header("Selection UI")]
    [SerializeField] private SelectionPanelUI selectionPanelUI; // Panel that displays selected units

    [Header("Layers")]
    [SerializeField] private LayerMask selectableLayer; // Units/buildings layer
    [SerializeField] private LayerMask groundLayer;     // Ground layer (click to move)
    [SerializeField] private LayerMask resourceLayer;   // Resource layer (trees, mines, etc.)

    [Header("Camera & Input")]
    [SerializeField] private Camera mainCamera;     // Main camera reference
    [SerializeField] private float dragThreshold = 10f; // Pixels to move before drag starts
    [SerializeField] private float gatherClickRadius = 2f; // Radius around ground click to detect resources

    private Vector2 startPosition;        // Mouse pos when left click started
    private Vector2 currentMousePosition; // Updated every frame
    private bool isDragging = false;      // True if currently dragging box
    private bool isLeftPressed = false;   // True while left mouse held

    private readonly List<Selectable> selectedObjects = new(); // Current selection
    private readonly List<Selectable> allSelectables = new();  // All selectable objects in scene

    // Events
    public event Action<int> OnSelectionChanged; // Fired when selection count changes
    public event Action<Vector3> OnGroundClick;  // Fired when ground is clicked

    private void Awake()
    {
        // Gather all selectable objects present at scene start
        allSelectables.AddRange(FindObjectsByType<Selectable>(FindObjectsSortMode.None));
    }

    private void Start()
    {
        selectionBoxUI.gameObject.SetActive(false); // Hide box initially
        DeselectAll(); // Ensure no objects are selected on start
    }

    private void OnEnable()
    {
        // Subscribe to InputManager events
        InputManager.OnPointerPositionChanged += OnPointerPositionChanged;
        InputManager.OnRightClick += HandleRightClick;
        InputManager.OnLeftPress += HandleLeftPress;
        InputManager.OnLeftRelease += HandleLeftRelease;

        OnSelectionChanged += OnSelectionChangedHandler;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        InputManager.OnPointerPositionChanged -= OnPointerPositionChanged;
        InputManager.OnRightClick -= HandleRightClick;
        InputManager.OnLeftPress -= HandleLeftPress;
        InputManager.OnLeftRelease -= HandleLeftRelease;
        OnSelectionChanged -= OnSelectionChangedHandler;
    }

    private void OnPointerPositionChanged(Vector2 pointerPos)
    {
        currentMousePosition = pointerPos; // Update mouse pos on move
    }

    private void HandleLeftPress()
    {
        isLeftPressed = true;
        startPosition = currentMousePosition;
        isDragging = false; // Reset drag flag
        selectionBoxUI.gameObject.SetActive(false); // Hide box until drag starts
    }

    private void HandleLeftRelease()
    {
        isLeftPressed = false;

        if (isDragging)
        {
            SelectObjectsInBox(); // Drag selection
        }
        else
        {
            TrySelectSingleObjectOrDeselect(); // Single-click selection/deselect
        }

        isDragging = false;
        selectionBoxUI.gameObject.SetActive(false); // Hide UI box
    }

    private void Update()
    {
        // Check if drag should start (moved enough while holding left)
        if (isLeftPressed && !isDragging && Vector2.Distance(currentMousePosition, startPosition) > dragThreshold)
        {
            isDragging = true;
            selectionBoxUI.gameObject.SetActive(true);
        }

        // If dragging, update the visual UI box
        if (isDragging)
            UpdateSelectionBoxUI();
    }

    /// <summary>
    /// Updates selection rectangle UI during drag.
    /// Converts screen positions to canvas local coordinates.
    /// </summary>
    private void UpdateSelectionBoxUI()
    {
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
    /// Selects all objects that fall inside drag selection rectangle.
    /// </summary>
    private void SelectObjectsInBox()
    {
        DeselectAll();

        // Build selection rectangle from UI box
        Rect selectionRect = new Rect(
            selectionBoxUI.anchoredPosition - selectionBoxUI.sizeDelta / 2f,
            selectionBoxUI.sizeDelta);

        foreach (Selectable selectable in allSelectables)
        {
            if (selectable == null) continue;

            // Convert object world pos to screen ? canvas local pos
            Vector2 screenPos = mainCamera.WorldToScreenPoint(selectable.transform.position);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint);

            // If inside rectangle, select
            if (selectionRect.Contains(localPoint))
            {
                selectable.Select();
                selectedObjects.Add(selectable);
            }
        }

        OnSelectionChanged?.Invoke(selectedObjects.Count);
    }

    /// <summary>
    /// Handles single-click selection of a unit/building, or deselection on ground click.
    /// </summary>
    private void TrySelectSingleObjectOrDeselect()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        // First try to hit a selectable unit/building
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            if (hit.collider.TryGetComponent(out Selectable selectable))
            {
                DeselectAll();
                selectable.Select();
                selectedObjects.Add(selectable);

                // Buildings open UI panel on select
                if (selectable is SelectableBuilding building)
                    building.OpenPanel();

                OnSelectionChanged?.Invoke(selectedObjects.Count);
                return;
            }
        }

        // If nothing hit, check ground click ? deselect all
        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundLayer))
        {
            DeselectAll();
            OnSelectionChanged?.Invoke(selectedObjects.Count);
        }
    }

    /// <summary>
    /// Deselect all currently selected objects.
    /// </summary>
    private void DeselectAll()
    {
        foreach (Selectable selectable in selectedObjects)
            selectable?.Deselect();

        selectedObjects.Clear();
    }

    /// <summary>
    /// Handles right-click command: move or gather resources.
    /// </summary>
    public void HandleRightClick(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // Ground click?
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // Check for nearby resource node
            Collider[] hits = Physics.OverlapSphere(hit.point, gatherClickRadius, resourceLayer);

            if (hits.Length > 0)
            {
                ResourceNode resourceNode = hits[0].GetComponent<ResourceNode>();

                if (resourceNode != null)
                {
                    foreach (var selectable in selectedObjects)
                    {
                        if (selectable == null) continue;

                        if (selectable.TryGetComponent<ResourceGathering>(out var gather))
                            gather.StartGathering(resourceNode);
                        else if (selectable.TryGetComponent<UnitMovement>(out var mover))
                            mover.MoveTo(resourceNode.transform.position, isPlayerCommand: true);
                    }
                    return; // Skip movement if resource found
                }
            }
            else
            {
                // Move command to clicked ground point
                SendMoveCommand(hit.point);
            }
        }
    }

    /// <summary>
    /// Sends move command to all selected units (with formation spread if multiple).
    /// </summary>
    private void SendMoveCommand(Vector3 point)
    {
        selectedObjects.RemoveAll(o => o == null); // Cleanup null refs

        int unitCount = selectedObjects.Count;
        float radius = 1.5f; // Formation spacing

        for (int i = 0; i < unitCount; i++)
        {
            // Spread units in a circle if multiple are selected
            Vector3 targetPosition = unitCount == 1 ? point : new Vector3(
                point.x + Mathf.Cos(i * Mathf.PI * 2f / unitCount) * radius,
                point.y,
                point.z + Mathf.Sin(i * Mathf.PI * 2f / unitCount) * radius
            );

            if (selectedObjects[i].TryGetComponent<ResourceGathering>(out var gather))
                gather.MoveTo(targetPosition);
            else if (selectedObjects[i].TryGetComponent<UnitMovement>(out var mover))
                mover.MoveTo(targetPosition, isPlayerCommand: true);
        }

        OnGroundClick?.Invoke(point);
    }

    /// <summary>
    /// Register new selectable object dynamically (e.g. spawned unit).
    /// </summary>
    public void RegisterSelectable(Selectable selectable)
    {
        if (!allSelectables.Contains(selectable))
            allSelectables.Add(selectable);
    }

    /// <summary>
    /// Unregister destroyed/removed selectable.
    /// </summary>
    public void UnregisterSelectable(Selectable selectable)
    {
        allSelectables.Remove(selectable);
    }

    /// <summary>
    /// Updates UI panel when selection changes.
    /// </summary>
    private void OnSelectionChangedHandler(int count)
    {
        if (selectionPanelUI != null)
        {
            List<Unit> selectedUnits = new List<Unit>();
            List<SelectableBuilding> selectedBuildings = new List<SelectableBuilding>();

            foreach (var sel in selectedObjects)
            {
                if (sel.TryGetComponent<Unit>(out var unit))
                    selectedUnits.Add(unit);
                else if (sel is SelectableBuilding building)
                    selectedBuildings.Add(building);
            }

            selectionPanelUI.UpdateSelection(selectedUnits, selectedBuildings);
        }
    }

}
