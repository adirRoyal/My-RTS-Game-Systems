using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour
{
    public event Action<Vector3> OnGroundClick; // event for when player clicks on ground

    [Header("UI References")]
    [SerializeField] private RectTransform selectionBoxUI; // the UI box that shows drag selection
    [SerializeField] private Canvas canvas; // canvas used to draw selection box

    [Header("Layers")]
    [SerializeField] private LayerMask selectableLayer; // layer of units/buildings
    [SerializeField] private LayerMask groundLayer; // layer of the ground
    [SerializeField] private LayerMask resourceLayer; // layer of resources (trees, etc)

    [Header("Camera & Input")]
    [SerializeField] private Camera mainCamera; // main camera reference
    [SerializeField] private float dragThreshold = 10f; // how much mouse must move to start drag
    [SerializeField] private float gatherClickRadius = 2f; // radius for resource click detection

    private Vector2 startPosition; // mouse position when left click pressed
    private Vector2 currentMousePosition; // updated mouse position
    private bool isDragging = false; // true if user is dragging a selection box
    private bool isLeftPressed = false; // true while left mouse is pressed

    private readonly List<Selectable> selectedObjects = new(); // currently selected units
    private readonly List<Selectable> allSelectables = new(); // all selectable objects in scene

    public event Action<int> OnSelectionChanged; // event for UI or other systems

    private void Awake()
    {
        // find all selectable objects in scene and store them
        allSelectables.AddRange(FindObjectsByType<Selectable>(FindObjectsSortMode.None));
    }

    private void Start()
    {
        selectionBoxUI.gameObject.SetActive(false); // hide box at start
        DeselectAll(); // clear any selection
    }

    private void OnEnable()
    {
        // subscribe to input events
        InputManager.OnPointerPositionChanged += OnPointerPositionChanged;
        InputManager.OnRightClick += HandleRightClick;
        InputManager.OnLeftPress += HandleLeftPress;
        InputManager.OnLeftRelease += HandleLeftRelease;
    }

    private void OnDisable()
    {
        // unsubscribe to avoid memory leaks
        InputManager.OnPointerPositionChanged -= OnPointerPositionChanged;
        InputManager.OnRightClick -= HandleRightClick;
        InputManager.OnLeftPress -= HandleLeftPress;
        InputManager.OnLeftRelease -= HandleLeftRelease;
    }

    private void OnPointerPositionChanged(Vector2 pointerPos)
    {
        currentMousePosition = pointerPos; // update mouse position
    }

    private void HandleLeftPress()
    {
        isLeftPressed = true; // left button down
        startPosition = currentMousePosition; // store start point
        isDragging = false; // reset dragging
        selectionBoxUI.gameObject.SetActive(false); // hide box initially
    }

    private void HandleLeftRelease()
    {
        isLeftPressed = false; // left button released

        if (isDragging)
        {
            SelectObjectsInBox(); // select all objects in box
        }
        else
        {
            TrySelectSingleObjectOrDeselect(); // normal single click selection
        }

        isDragging = false; // reset dragging
        selectionBoxUI.gameObject.SetActive(false); // hide box
    }

    private void Update()
    {
        // only start drag if left button is pressed and distance is over threshold
        if (isLeftPressed && !isDragging && Vector2.Distance(currentMousePosition, startPosition) > dragThreshold)
        {
            isDragging = true; // now dragging
            selectionBoxUI.gameObject.SetActive(true); // show UI box
        }

        if (isDragging)
            UpdateSelectionBoxUI(); // update size/position of selection box
    }

    private void UpdateSelectionBoxUI()
    {
        // convert screen points to local canvas points
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            startPosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 startLocal);

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvas.transform as RectTransform,
            currentMousePosition, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
            out Vector2 currentLocal);

        // calculate size and center
        Vector2 size = currentLocal - startLocal;
        selectionBoxUI.anchoredPosition = startLocal + size / 2f;
        selectionBoxUI.sizeDelta = new Vector2(Mathf.Abs(size.x), Mathf.Abs(size.y));
    }

    private void SelectObjectsInBox()
    {
        DeselectAll(); // clear previous selection

        // define the selection rectangle
        Rect selectionRect = new Rect(
            selectionBoxUI.anchoredPosition - selectionBoxUI.sizeDelta / 2f,
            selectionBoxUI.sizeDelta);

        foreach (Selectable selectable in allSelectables)
        {
            if (selectable == null) continue;

            // get object screen position
            Vector2 screenPos = mainCamera.WorldToScreenPoint(selectable.transform.position);

            // convert to local canvas space
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvas.transform as RectTransform,
                screenPos, canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : mainCamera,
                out Vector2 localPoint);

            // check if object inside selection box
            if (selectionRect.Contains(localPoint))
            {
                selectable.Select(); // select object
                selectedObjects.Add(selectable);
            }
        }

        OnSelectionChanged?.Invoke(selectedObjects.Count); // notify UI
    }

    private void TrySelectSingleObjectOrDeselect()
    {
        // raycast to see if a unit/building is clicked
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, selectableLayer))
        {
            if (hit.collider.TryGetComponent(out Selectable selectable))
            {
                DeselectAll(); // clear previous selection
                selectable.Select(); // select clicked object
                selectedObjects.Add(selectable);

                if (selectable is SelectableBuilding building)
                    building.OpenPanel(); // open UI panel for building

                OnSelectionChanged?.Invoke(selectedObjects.Count);
                return;
            }
        }

        // check ground click to deselect
        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundLayer))
        {
            DeselectAll();
            OnSelectionChanged?.Invoke(selectedObjects.Count);
        }
    }

    private void DeselectAll()
    {
        // deselect everything
        foreach (Selectable selectable in selectedObjects)
            selectable?.Deselect();

        selectedObjects.Clear(); // clear list
    }

    public void HandleRightClick(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);

        // raycast ground first
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // check for resources near clicked point
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
                            gather.StartGathering(resourceNode); // gather resource
                        else if (selectable.TryGetComponent<UnitMovement>(out var mover))
                            mover.MoveTo(resourceNode.transform.position, isPlayerCommand: true); // move unit
                    }
                    return;
                }
            }
            else
            {
                SendMoveCommand(hit.point); // move units to clicked point
            }
        }
    }

    private void SendMoveCommand(Vector3 point)
    {
        selectedObjects.RemoveAll(o => o == null); // cleanup nulls

        int unitCount = selectedObjects.Count;
        float radius = 1.5f; // formation spacing

        for (int i = 0; i < unitCount; i++)
        {
            // calculate formation offset if multiple units
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

        OnGroundClick?.Invoke(point); // notify listeners
    }

    public void RegisterSelectable(Selectable selectable)
    {
        // add new selectable object dynamically
        if (!allSelectables.Contains(selectable))
            allSelectables.Add(selectable);
    }

    public void UnregisterSelectable(Selectable selectable)
    {
        // remove selectable object dynamically
        allSelectables.Remove(selectable);
    }
}
