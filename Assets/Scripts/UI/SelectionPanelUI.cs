using System.Collections.Generic;
using UnityEngine;

public class SelectionPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SingleUnitUI singleUnitUI;
    [SerializeField] private GameObject multiUnitPanel;
    [SerializeField] private GameObject unitIconPrefab;
    [SerializeField] private Transform contentParent;

    private List<UnitIconUI> currentIcons = new();
    private List<Unit> trackedUnits = new();
    private List<SelectableBuilding> trackedBuildings = new();

    private Unit currentSingleUnit = null;
    private BuildingComponent currentSingleBuilding = null;

    public void UpdateSelection(List<Unit> selectedUnits, List<SelectableBuilding> selectedBuildings = null)
    {
        // --- נקה מאזיני Unit ---
        foreach (var unit in trackedUnits)
            if (unit != null) unit.Health.OnDeath -= OnUnitDeath;

        trackedUnits = new List<Unit>(selectedUnits);
        foreach (var unit in trackedUnits)
            unit.Health.OnDeath += OnUnitDeath;

        // --- נקה מאזיני Building ---
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

    private void OnUnitDeath()
    {
        trackedUnits.RemoveAll(u => u == null || u.Health.IsDead());
        RefreshUI();
    }

    private void RefreshUI()
    {
        // נקה MultiUnit icons ישנים
        foreach (var icon in currentIcons) Destroy(icon.gameObject);
        currentIcons.Clear();

        // נקה מאזינים ישנים
        if (currentSingleUnit != null)
            currentSingleUnit.Health.OnHealthChanged -= UpdateSingleUnitHealth;
        if (currentSingleBuilding != null && currentSingleBuilding.Health != null)
            currentSingleBuilding.Health.OnHealthChanged -= UpdateSingleUnitHealth;

        currentSingleUnit = null;
        currentSingleBuilding = null;

        // --- אין פריטים נבחרים ---
        if (trackedUnits.Count == 0 && trackedBuildings.Count == 0)
        {
            singleUnitUI.unitImage.sprite = null;
            singleUnitUI.healthFill.fillAmount = 0f;
            singleUnitUI.unitName.text = "";
            return;
        }

        // --- Single Panel ---
        singleUnitUI.unitImage.sprite = null;
        singleUnitUI.healthFill.fillAmount = 0f;
        singleUnitUI.unitName.text = "";

        if (trackedUnits.Count > 0)
        {
            // מציג את היחידה הראשונה
            Unit firstUnit = trackedUnits[0];
            currentSingleUnit = firstUnit;

            singleUnitUI.unitImage.sprite = firstUnit.Data.unitIcon;
            singleUnitUI.healthFill.fillAmount =
                (float)firstUnit.Health.GetCurrentHealth() / firstUnit.Health.GetMaxHealth();
            singleUnitUI.unitName.text = firstUnit.Data.unitName;

            firstUnit.Health.OnHealthChanged += UpdateSingleUnitHealth;
        }
        else if (trackedBuildings.Count > 0)
        {
            // מציג את הבניין הראשון
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
                singleUnitUI.healthFill.fillAmount = 1f;
            }
        }

        // --- Multi Unit Panel ---
        foreach (var unit in trackedUnits)
        {
            GameObject go = Instantiate(unitIconPrefab, contentParent);
            UnitIconUI iconUI = go.GetComponent<UnitIconUI>();
            iconUI.SetUnit(unit);
            currentIcons.Add(iconUI);
        }
    }

    // ? עדכון Health מותאם רק ליחידה או בניין שנבחר
    private void UpdateSingleUnitHealth(int current, int max)
    {
        if (currentSingleUnit != null)
        {
            singleUnitUI.healthFill.fillAmount = (float)current / max;
        }
        else if (currentSingleBuilding != null)
        {
            singleUnitUI.healthFill.fillAmount = (float)current / max;
        }
    }

}
