using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage;
    private HealthSystem healthSystem;

    public void SetHealthSystem(HealthSystem system)
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;

        healthSystem = system;

        if (healthSystem != null)
        {
            healthSystem.OnHealthChanged += UpdateHealthBar;
            UpdateHealthBar(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        fillImage.fillAmount = (float)currentHealth / maxHealth;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;
    }
}
