using System;
using UnityEngine;

public class SelectableUnit : MonoBehaviour
{
    public static event Action<SelectableUnit> OnSelected;
    public static event Action<SelectableUnit> OnDeselected;

    [SerializeField] private GameObject selectionVisual;

    private void Awake()
    {
        Deselect();
    }

    public void Select()
    {
        selectionVisual.SetActive(true);
        OnSelected?.Invoke(this);
    }

    public void Deselect()
    {
        selectionVisual.SetActive(false);
        OnDeselected?.Invoke(this);
    }
}
