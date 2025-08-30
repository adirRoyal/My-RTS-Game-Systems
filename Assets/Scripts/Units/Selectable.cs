using System;
using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    public static event Action<Selectable> OnSelected;
    public static event Action<Selectable> OnDeselected;

    [SerializeField] private GameObject selectionVisual;

    protected virtual void Awake()
    {
        Deselect();
    }

    public virtual void Select()
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(true);

        OnSelected?.Invoke(this);
    }

    public virtual void Deselect()
    {
        if (selectionVisual != null)
            selectionVisual.SetActive(false);

        OnDeselected?.Invoke(this);
    }

    private void OnDestroy()
    {
        var handler = FindFirstObjectByType<UnitSelectionHandler>();
        if (handler != null)
            handler.UnregisterSelectable(this);
    }
}
