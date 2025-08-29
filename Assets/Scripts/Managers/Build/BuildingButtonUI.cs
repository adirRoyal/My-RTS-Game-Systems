using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BuildingButtonUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image iconImage; // UI Image for building icon
    [SerializeField] private TextMeshProUGUI nameText; // UI text for building name
    [SerializeField] private Button button; // Button to trigger placement

    [Header("Tooltip")]
    [SerializeField] private GameObject tooltipPrefab; // Prefab shown on hover
    [SerializeField] private Transform tooltipParent; // Parent for tooltip, optional

    private BuildingData buildingData; // Reference to this building's data
    private BuildingPlacementSystem placementSystem; // Reference to the placement system
    private GameObject tooltipInstance; // Active tooltip instance

    /// <summary>
    /// Setup the button UI with building data and placement system reference.
    /// </summary>
    public void Setup(BuildingData data, BuildingPlacementSystem system)
    {
        buildingData = data;
        placementSystem = system;

        // --- Auto-find tooltip parent if not assigned ---
        if (tooltipParent == null)
        {
            var buildMenu = FindFirstObjectByType<BuildMenuUI>();
            if (buildMenu != null)
                tooltipParent = buildMenu.transform;
        }

        // --- Update UI elements ---
        if (iconImage != null)
            iconImage.sprite = buildingData.icon;

        if (nameText != null)
            nameText.text = buildingData.buildingName;

        // --- Setup button click ---
        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClick);

        // --- Setup hover events for tooltip ---
        var trigger = button.gameObject.AddComponent<UnityEngine.EventSystems.EventTrigger>();

        var entryEnter = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerEnter
        };
        entryEnter.callback.AddListener((_) => ShowTooltip());

        var entryExit = new UnityEngine.EventSystems.EventTrigger.Entry
        {
            eventID = UnityEngine.EventSystems.EventTriggerType.PointerExit
        };
        entryExit.callback.AddListener((_) => HideTooltip());

        trigger.triggers.Add(entryEnter);
        trigger.triggers.Add(entryExit);
    }

    /// <summary>
    /// Called when the button is clicked: start placement of this building.
    /// </summary>
    private void OnClick()
    {
        if (placementSystem == null || buildingData == null) return;

        placementSystem.StartPlacement(buildingData);
    }

    /// <summary>
    /// Shows the tooltip for this building.
    /// </summary>
    private void ShowTooltip()
    {
        if (tooltipPrefab != null && tooltipInstance == null)
        {
            tooltipInstance = Instantiate(tooltipPrefab, tooltipParent ?? transform);

            var tooltipUI = tooltipInstance.GetComponent<BuildingTooltipUI>();
            if (tooltipUI != null)
                tooltipUI.Setup(buildingData);
        }
    }

    /// <summary>
    /// Hides and destroys the tooltip instance.
    /// </summary>
    private void HideTooltip()
    {
        if (tooltipInstance != null)
        {
            Destroy(tooltipInstance);
            tooltipInstance = null;
        }
    }

    // --- Optional Improvements ---
    // 1. Cache the EventTrigger to avoid adding multiple triggers if Setup is called multiple times.
    // 2. Add null checks for button, iconImage, nameText to avoid runtime errors.
    // 3. Consider pooling tooltip instances for better performance instead of instantiating/destroying every time.
}
