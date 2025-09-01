using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    // Reference to the UI Image that visually represents the health (usually the "fill" part of a bar).
    [SerializeField] private Image fillImage;

    // Reference to the HealthSystem this bar is bound to.
    // This allows the HealthBar to dynamically reflect changes in health.
    private HealthSystem healthSystem;

    /// <summary>
    /// Binds a HealthSystem instance to this HealthBar so the UI can update automatically.
    /// Handles cleanup of previous subscriptions to avoid memory leaks or dangling references.
    /// </summary>
    /// <param name="system">The HealthSystem instance to observe.</param>
    public void SetHealthSystem(HealthSystem system)
    {
        // If already bound to a different HealthSystem, unsubscribe from its event first
        // to prevent multiple subscriptions or referencing destroyed objects.
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;

        // Assign the new system
        healthSystem = system;

        // Subscribe to the new system’s health change event (if not null).
        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthBar;

            // Immediately update the UI so the bar reflects the correct health state
            // without waiting for the next change event.
            UpdateHealthBar(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    /// <summary>
    /// Callback method that updates the health bar UI.
    /// Invoked whenever the health system triggers its OnHealthChanged event.
    /// </summary>
    /// <param name="currentHealth">The current health value.</param>
    /// <param name="maxHealth">The maximum health value.</param>
    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // Convert current health to a normalized [0..1] value and update fill amount.
        // This directly drives the UI bar’s appearance.
        fillImage.fillAmount = (float)currentHealth / maxHealth;
    }

    /// <summary>
    /// Ensures proper event unsubscription when this HealthBar is destroyed.
    /// Prevents dangling delegates pointing to destroyed objects.
    /// </summary>
    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;
    }
}
