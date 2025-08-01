using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler selectionHandler;
    [SerializeField] private TextMeshProUGUI selectedUnitsCountText;

    private void OnEnable()
    {
        selectionHandler.OnSelectionChanged += UpdateSelectedUnitsCount;
    }

    private void OnDisable()
    {
        selectionHandler.OnSelectionChanged -= UpdateSelectedUnitsCount;
    }

    private void UpdateSelectedUnitsCount(int count)
    {
        selectedUnitsCountText.text = $"Selected Units: {count}";
    }
}
