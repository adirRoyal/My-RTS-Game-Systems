using System;
using UnityEngine;

/// <summary>
/// Base class for anything that can be selected (units, buildings, etc.).
/// Handles visual selection indicator and selection events.
/// </summary>
public abstract class Selectable : MonoBehaviour
{
    // Static events for when any selectable object is selected/deselected
    public static event Action<Selectable> OnSelected;
    public static event Action<Selectable> OnDeselected;

    [Header("Visuals")]
    [SerializeField] private GameObject selectionVisual; // Highlight or outline object

    /// <summary>
    /// Called when object awakens.
    /// Ensures the object starts deselected.
    /// </summary>
    protected virtual void Awake()
    {
        Deselect(); // Hide selection visual at start
    }

    /// <summary>
    /// Select this object
    /// Activates visual and fires event.
    /// </summary>
    public virtual void Select()
    {
        // Show selection visuals if assigned
        if (selectionVisual != null)
            selectionVisual.SetActive(true);

        // Notify any listeners that this object was selected
        OnSelected?.Invoke(this);
    }

    /// <summary>
    /// Deselect this object
    /// Deactivates visual and fires event.
    /// </summary>
    public virtual void Deselect()
    {
        // Hide selection visuals
        if (selectionVisual != null)
            selectionVisual.SetActive(false);

        // Notify any listeners that this object was deselected
        OnDeselected?.Invoke(this);
    }

    /// <summary>
    /// Called when object is destroyed.
    /// Unregisters from the selection system to avoid dangling references.
    /// </summary>
    private void OnDestroy()
    {
        // Find the selection handler in the scene
        var handler = FindFirstObjectByType<UnitSelectionHandler>();
        if (handler != null)
            handler.UnregisterSelectable(this);
    }
}
