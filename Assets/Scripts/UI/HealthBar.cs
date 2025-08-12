using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Image with Fill method (Horizontal)
    private HealthSystem healthSystem;

    private void Start()
    {
        // Find HealthSystem component on this object or its parents
        healthSystem = GetComponentInParent<HealthSystem>();

        if (healthSystem != null)
        {
            // Subscribe to health change event
            healthSystem.OnHealthChanged += UpdateHealthBar;

            // Update health bar UI at start
            UpdateHealthBar(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        // Calculate fill amount (0 to 1)
        float fillAmount = (float)currentHealth / maxHealth;

        // Update image fill amount
        fillImage.fillAmount = fillAmount;
    }

    private void OnDestroy()
    {
        // Unsubscribe from event when this object is destroyed to avoid memory leaks
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;
    }
}
