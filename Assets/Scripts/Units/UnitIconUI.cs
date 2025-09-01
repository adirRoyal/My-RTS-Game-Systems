using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Represents a small UI element for a single unit in a selection panel.
/// Shows unit icon and health bar.
/// </summary>
public class UnitIconUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private Image unitIcon;   // Image showing the unit's icon/sprite
    [SerializeField] private Image healthFill; // UI fill image to represent health

    private HealthSystem healthSystem;         // Reference to the unit's health system

    /// <summary>
    /// Assigns a unit to this UI element.
    /// Updates icon and subscribes to health changes.
    /// </summary>
    /// <param name="unit">The unit to display</param>
    public void SetUnit(Unit unit)
    {
        // --- Set unit icon ---
        if (unitIcon != null && unit.Data != null)
            unitIcon.sprite = unit.Data.unitIcon;

        // --- Unsubscribe from previous health system events if any ---
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealth;

        // --- Assign new health system ---
        healthSystem = unit.Health;

        // --- Subscribe to health changes and immediately update UI ---
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealth;
            UpdateHealth(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    /// <summary>
    /// Updates the health bar fill based on current and max health.
    /// </summary>
    /// <param name="current">Current health value</param>
    /// <param name="max">Maximum health value</param>
    private void UpdateHealth(int current, int max)
    {
        if (healthFill != null)
            healthFill.fillAmount = (float)current / max; // convert to 0-1 fill
    }

    /// <summary>
    /// Cleanup event subscription when the UI element is destroyed
    /// to avoid memory leaks or null reference calls.
    /// </summary>
    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealth;
    }
}
