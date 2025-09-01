using System.Collections.Generic;
using UnityEngine;

public class SelectionPanelUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private SingleUnitUI singleUnitUI; // כל ההפניות ל־single unit
    [SerializeField] private GameObject multiUnitPanel;
    [SerializeField] private GameObject unitIconPrefab;
    [SerializeField] private Transform contentParent;

    private List<UnitIconUI> currentIcons = new();
    private List<Unit> trackedUnits = new(); // יחידות שנבחרו ומעקב אחריהן

    private void Awake()
    {
        // שני הפאנלים פעילים תמיד
        singleUnitUI.panel.SetActive(true);
        multiUnitPanel.SetActive(true);
    }

    /// <summary>
    /// מעדכן את בחירת היחידות ומציג את ה-UI
    /// </summary>
    public void UpdateSelection(List<Unit> selectedUnits)
    {
        // ביטול הרשאות אירועים ישנים
        foreach (var unit in trackedUnits)
        {
            if (unit != null)
            {
                unit.Health.OnDeath -= OnUnitDeath;
            }
        }

        trackedUnits = new List<Unit>(selectedUnits); // מעקב אחרי היחידות החדשות

        // הרשמת אירועי OnDeath לכל יחידה
        foreach (var unit in trackedUnits)
        {
            unit.Health.OnDeath += OnUnitDeath;
        }

        RefreshUI();
    }

    /// <summary>
    /// מתבצע כאשר יחידה מתה
    /// </summary>
    private void OnUnitDeath()
    {
        // מחיקת יחידות מתות מהרשימה
        trackedUnits.RemoveAll(u => u == null || u.Health.IsDead());

        // עדכון ה-UI אחרי מחיקה
        RefreshUI();
    }

    /// <summary>
    /// ריענון ה-UI לפי trackedUnits
    /// </summary>
    private void RefreshUI()
    {
        // ניקוי MultiUnitPanel
        foreach (var icon in currentIcons)
            Destroy(icon.gameObject);
        currentIcons.Clear();

        if (trackedUnits.Count == 0)
        {
            // אין בחירה, ריקון SingleUnitPanel
            singleUnitUI.unitImage.sprite = null;
            singleUnitUI.healthFill.fillAmount = 0f;
            singleUnitUI.unitName.text = "";
            return;
        }

        // --- SingleUnitPanel --- 
        Unit firstUnit = trackedUnits[0];
        singleUnitUI.unitImage.sprite = firstUnit.Data.unitIcon;
        singleUnitUI.healthFill.fillAmount = (float)firstUnit.Health.GetCurrentHealth() / firstUnit.Health.GetMaxHealth();
        singleUnitUI.unitName.text = firstUnit.Data.unitName;

        // עדכון Health בזמן אמת
        firstUnit.Health.OnHealthChanged -= UpdateSingleUnitHealth;
        firstUnit.Health.OnHealthChanged += UpdateSingleUnitHealth;

        // --- MultiUnitPanel --- 
        foreach (var unit in trackedUnits)
        {
            GameObject go = Instantiate(unitIconPrefab, contentParent);
            UnitIconUI iconUI = go.GetComponent<UnitIconUI>();
            iconUI.SetUnit(unit);
            currentIcons.Add(iconUI);
        }
    }

    private void UpdateSingleUnitHealth(int current, int max)
    {
        singleUnitUI.healthFill.fillAmount = (float)current / max;
    }
}
