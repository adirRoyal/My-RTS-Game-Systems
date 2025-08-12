using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIManager : MonoBehaviour
{
    [SerializeField] private UnitSelectionHandler selectionHandler;  // Reference to selection system
    [SerializeField] private TextMeshProUGUI selectedUnitsCountText; // UI text to show selected count

    private void OnEnable()
    {
        // Subscribe to event when selection changes
        selectionHandler.OnSelectionChanged += UpdateSelectedUnitsCount;
    }

    private void OnDisable()
    {
        // Unsubscribe to avoid memory leaks
        selectionHandler.OnSelectionChanged -= UpdateSelectedUnitsCount;
    }

    private void UpdateSelectedUnitsCount(int count)
    {
        // Update the UI text with the new count of selected units
        selectedUnitsCountText.text = $"Selected Units: {count}";
    }
}
