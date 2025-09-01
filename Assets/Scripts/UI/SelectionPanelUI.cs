using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles the Selection Panel UI for both Units and Buildings.
/// Displays single selected item info and multi-unit icons dynamically.
/// </summary>
public class SelectionPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SingleUnitUI singleUnitUI; // Panel for single unit/building display
    [SerializeField] private GameObject multiUnitPanel; // Panel to show multiple unit icons
    [SerializeField] private GameObject unitIconPrefab; // Prefab for multi-unit icons
    [SerializeField] private Transform contentParent;   // Parent transform for instantiated icons

    private List<UnitIconUI> currentIcons = new();     // Currently displayed unit icons
    private List<Unit> trackedUnits = new();          // Currently selected units
    private List<SelectableBuilding> trackedBuildings = new(); // Currently selected buildings

    private Unit currentSingleUnit = null;           // First selected unit (for single panel)
    private BuildingComponent currentSingleBuilding = null; // First selected building (for single panel)

    /// <summary>
    /// Update the selection UI with new units and buildings.
    /// </summary>
    public void UpdateSelection(List<Unit> selectedUnits, List<SelectableBuilding> selectedBuildings = null)
    {
        // --- Remove old unit listeners ---
        foreach (var unit in trackedUnits)
            if (unit != null) unit.Health.OnDeath -= OnUnitDeath;

        // Track new units
        trackedUnits = new List<Unit>(selectedUnits);
        foreach (var unit in trackedUnits)
            unit.Health.OnDeath += OnUnitDeath;

        // --- Update building selection ---
        if (selectedBuildings != null)
        {
            trackedBuildings = new List<SelectableBuilding>(selectedBuildings);
        }
        else
        {
            trackedBuildings.Clear();
        }

        RefreshUI();
    }

    /// <summary>
    /// Called when any tracked unit dies. Removes dead units and refreshes UI.
    /// </summary>
    private void OnUnitDeath()
    {
        trackedUnits.RemoveAll(u => u == null || u.Health.IsDead());
        RefreshUI();
    }

    /// <summary>
    /// Refreshes the UI: clears old icons, updates single unit/building panel.
    /// </summary>
    private void RefreshUI()
    {
        // --- Clear Multi-Unit icons ---
        foreach (var icon in currentIcons) Destroy(icon.gameObject);
        currentIcons.Clear();

        // --- Remove old health listeners ---
        if (currentSingleUnit != null)
            currentSingleUnit.Health.OnHealthChanged -= UpdateSingleUnitHealth;
        if (currentSingleBuilding != null && currentSingleBuilding.Health != null)
            currentSingleBuilding.Health.OnHealthChanged -= UpdateSingleUnitHealth;

        currentSingleUnit = null;
        currentSingleBuilding = null;

        // --- No selection, clear single panel ---
        if (trackedUnits.Count == 0 && trackedBuildings.Count == 0)
        {
            singleUnitUI.unitImage.sprite = null;
            singleUnitUI.healthFill.fillAmount = 0f;
            singleUnitUI.unitName.text = "";
            return;
        }

        // --- Reset single panel UI before assigning ---
        singleUnitUI.unitImage.sprite = null;
        singleUnitUI.healthFill.fillAmount = 0f;
        singleUnitUI.unitName.text = "";

        // --- Single Unit display ---
        if (trackedUnits.Count > 0)
        {
            Unit firstUnit = trackedUnits[0];
            currentSingleUnit = firstUnit;

            singleUnitUI.unitImage.sprite = firstUnit.Data.unitIcon;
            singleUnitUI.healthFill.fillAmount =
                (float)firstUnit.Health.GetCurrentHealth() / firstUnit.Health.GetMaxHealth();
            singleUnitUI.unitName.text = firstUnit.Data.unitName;

            // Subscribe to health changes
            firstUnit.Health.OnHealthChanged += UpdateSingleUnitHealth;
        }
        // --- Single Building display ---
        else if (trackedBuildings.Count > 0)
        {
            SelectableBuilding building = trackedBuildings[0];
            BuildingComponent bc = building.GetComponent<BuildingComponent>();
            currentSingleBuilding = bc;

            singleUnitUI.unitImage.sprite = bc.Data.icon;
            singleUnitUI.unitName.text = bc.Data.buildingName;

            if (bc.Health != null)
            {
                singleUnitUI.healthFill.fillAmount =
                    (float)bc.Health.GetCurrentHealth() / bc.Health.GetMaxHealth();
                bc.Health.OnHealthChanged += UpdateSingleUnitHealth;
            }
            else
            {
                // If no health system, just show full bar
                singleUnitUI.healthFill.fillAmount = 1f;
            }
        }

        // --- Multi Unit Panel ---
        foreach (var unit in trackedUnits)
        {
            GameObject go = Instantiate(unitIconPrefab, contentParent);
            UnitIconUI iconUI = go.GetComponent<UnitIconUI>();
            iconUI.SetUnit(unit); // Assign the unit to the icon
            currentIcons.Add(iconUI);
        }
    }

    /// <summary>
    /// Update the health bar of the currently displayed single unit/building.
    /// </summary>
    private void UpdateSingleUnitHealth(int current, int max)
    {
        if (currentSingleUnit != null || currentSingleBuilding != null)
        {
            singleUnitUI.healthFill.fillAmount = (float)current / max;
        }
    }
}
