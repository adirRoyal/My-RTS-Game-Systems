using System;
using System.Collections.Generic;
using UnityEngine;

public class UnitSelectionHandler : MonoBehaviour
{
    public event Action<Vector3> OnGroundClick;


    [SerializeField] private RectTransform selectionBoxUI;
    [SerializeField] private Canvas canvas;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera mainCamera;
    [SerializeField] private float dragThreshold = 10f;

    private Vector2 startPosition;
    private Vector2 currentMousePosition;
    private bool isDragging = false;

    private List<SelectableUnit> selectedUnits = new();

    // אירוע שיקרא כל פעם שהבחירה משתנה, עם כמות היחידות שנבחרו
    public event Action<int> OnSelectionChanged;

    private void Start()
    {
        selectionBoxUI.gameObject.SetActive(false);
        DeselectAll();
    }

    private void OnEnable()
    {
        InputManager.OnPointerPositionChanged += OnPointerPositionChanged;
        InputManager.OnRightClick += HandleRightClick;
    }

    private void OnDisable()
    {
        InputManager.OnPointerPositionChanged -= OnPointerPositionChanged;
        InputManager.OnRightClick -= HandleRightClick;
    }

    private void OnPointerPositionChanged(Vector2 pointerPos)
    {
        currentMousePosition = pointerPos;
    }

    private void Update()
    {
        // ניהול לחיצה שמאל - אפשר לשדרג בעתיד לקלט InputSystem מלא
        if (UnityEngine.InputSystem.Mouse.current.leftButton.wasPressedThisFrame)
        {
            startPosition = currentMousePosition;
            isDragging = false;
        }
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

    private void SelectUnitsInBox()
    {
        DeselectAll();

        Rect selectionRect = new Rect(
            selectionBoxUI.anchoredPosition - selectionBoxUI.sizeDelta / 2f,
            selectionBoxUI.sizeDelta);

        foreach (SelectableUnit unit in GameObject.FindObjectsByType<SelectableUnit>(UnityEngine.FindObjectsSortMode.None))
        {
            Vector2 screenPos = mainCamera.WorldToScreenPoint(unit.transform.position);
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

    private void TrySelectSingleUnitOrDeselect()
    {
        Ray ray = mainCamera.ScreenPointToRay(currentMousePosition);

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

        if (Physics.Raycast(ray, out RaycastHit groundHit, Mathf.Infinity, groundLayer))
        {
            DeselectAll();
            OnSelectionChanged?.Invoke(selectedUnits.Count);
        }
    }

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


    public void HandleRightClick(Vector2 mousePosition)
    {
        Ray ray = mainCamera.ScreenPointToRay(mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, groundLayer))
        {
            // לפני השימוש, ננקה יחידות שנהרסו
            selectedUnits.RemoveAll(unit => unit == null);

            int unitCount = selectedUnits.Count;
            float radius = 1.5f; // רדיוס הפיזור

            for (int i = 0; i < unitCount; i++)
            {
                Vector3 targetPosition;

                if (unitCount == 1)
                {
                    targetPosition = hit.point;
                }
                else
                {
                    float angle = i * Mathf.PI * 2f / unitCount;
                    float offsetX = Mathf.Cos(angle) * radius;
                    float offsetZ = Mathf.Sin(angle) * radius;

                    targetPosition = new Vector3(hit.point.x + offsetX, hit.point.y, hit.point.z + offsetZ);
                }

                if (selectedUnits[i].TryGetComponent<UnitMovement>(out UnitMovement mover))
                {
                    mover.MoveTo(targetPosition);
                }
            }

            OnGroundClick?.Invoke(hit.point);
        }
    }

}
