using UnityEngine;
using UnityEngine.UI;

public class UnitIconUI : MonoBehaviour
{
    [SerializeField] private Image unitIcon;
    [SerializeField] private Image healthFill;

    private HealthSystem healthSystem;

    public void SetUnit(Unit unit)
    {
        if (unitIcon != null && unit.Data != null)
            unitIcon.sprite = unit.Data.unitIcon;

        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealth;

        healthSystem = unit.Health;
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealth;
            UpdateHealth(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    private void UpdateHealth(int current, int max)
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)current / max;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealth;
    }
}
