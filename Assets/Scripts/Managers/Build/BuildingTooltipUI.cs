using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Handles the tooltip UI for a building button.
/// Shows name, cost, supply requirement, and build time.
/// </summary>
public class BuildingTooltipUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI titleText;     // Building name
    [SerializeField] private TextMeshProUGUI costText;      // Resource cost display
    [SerializeField] private TextMeshProUGUI supplyText;    // Population/supply requirement
    [SerializeField] private TextMeshProUGUI buildTimeText; // Build time display

    /// <summary>
    /// Populates the tooltip with data from a BuildingData object.
    /// </summary>
    public void Setup(BuildingData data)
    {
        if (data == null) return;

        // --- Title ---
        if (titleText)
            titleText.text = data.buildingName;

        // --- Cost ---
        string costString = "";
        if (data.costGold > 0) costString += $"Gold: {data.costGold}\n";
        if (data.costWood > 0) costString += $"Wood: {data.costWood}\n";
        if (costText)
            costText.text = costString.Trim(); // Remove trailing newline

        // --- Supply requirement ---
        if (supplyText)
        {
            supplyText.text = data.requiredPopulation > 0
                ? $"Requires: {data.requiredPopulation} supply"
                : "";
        }

        // --- Build time ---
        if (buildTimeText)
            buildTimeText.text = $"Build Time: {data.buildTime:0.0}s";
    }

    // --- Optional Improvements ---
    // 1. Add icons for Gold/Wood for better visual clarity.
    // 2. Highlight insufficient resources in red when previewing building.
    // 3. Animate tooltip appearance for smoother UI feedback.
}
