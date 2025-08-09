using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;
    private int currentHealth;

    public event Action OnDeath;
    public event Action<int, int> OnHealthChanged;

    private bool isDead = false;  // ��� ������ ������ ������

    private void Awake()
    {
        currentHealth = maxHealth;
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;  // �� ��� ��, �� �������

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            isDead = true;  // ������ ��������� ��
            OnDeath?.Invoke();

            // ����� ��������
            Destroy(gameObject);
        }
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
}
