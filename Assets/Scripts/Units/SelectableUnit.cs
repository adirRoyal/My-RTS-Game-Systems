using System;
using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
    // Static events that broadcast when any unit is selected or deselected.
    // Other systems (e.g., UI, game logic) can subscribe to react accordingly.
    public static event Action<SelectableUnit> OnSelected;
    public static event Action<SelectableUnit> OnDeselected;

    // Visual representation shown when this unit is selected
    [SerializeField] private GameObject selectionVisual;

    private void Awake()
    {
        // Ensure the unit starts in a deselected state when spawned/loaded
        Deselect();
    }

    /// <summary>
    /// Marks this unit as selected.
    /// Activates its visual indicator and notifies subscribers.
    /// </summary>
    public void Select()
    {
        selectionVisual.SetActive(true);
        OnSelected?.Invoke(this);
    }

    /// <summary>
    /// Marks this unit as deselected.
    /// Deactivates its visual indicator and notifies subscribers.
    /// </summary>
    public void Deselect()
    {
        selectionVisual.SetActive(false);
        OnDeselected?.Invoke(this);
    }

    private void OnDestroy()
    {
        var handler = FindObjectOfType<UnitSelectionHandler>();
        if (handler != null)
            handler.UnregisterUnit(this);
    }

}
