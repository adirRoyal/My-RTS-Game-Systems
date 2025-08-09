using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Image מסוג Filled (Horizontal)
    private HealthSystem healthSystem;

    private void Start()
    {
        // מחפש את HealthSystem אצל ההורה או באותו אובייקט
        healthSystem = GetComponentInParent<HealthSystem>();
        if (healthSystem != null)
        {
            // מנוי לאירוע שינוי חיים
            healthSystem.OnHealthChanged += UpdateHealthBar;
            // עדכון התחלי של הבר
            UpdateHealthBar(healthSystem.GetCurrentHealth(), healthSystem.GetMaxHealth());
        }
    }

    private void UpdateHealthBar(int currentHealth, int maxHealth)
    {
        float fillAmount = (float)currentHealth / maxHealth;
        fillImage.fillAmount = fillAmount;
    }

    private void OnDestroy()
    {
        if (healthSystem != null)
            healthSystem.OnHealthChanged -= UpdateHealthBar;
    }
}
