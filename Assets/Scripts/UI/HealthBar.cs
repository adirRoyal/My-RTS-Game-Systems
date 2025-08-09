using UnityEngine;
using UnityEngine.UI;

public class HealthBar : MonoBehaviour
{
    [SerializeField] private Image fillImage; // Image ���� Filled (Horizontal)
    private HealthSystem healthSystem;

    private void Start()
    {
        // ���� �� HealthSystem ��� ����� �� ����� �������
        healthSystem = GetComponentInParent<HealthSystem>();
        if (healthSystem != null)
        {
            // ���� ������ ����� ����
            healthSystem.OnHealthChanged += UpdateHealthBar;
            // ����� ����� �� ���
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
