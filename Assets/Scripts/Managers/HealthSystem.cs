using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;   // Max health value
    private int currentHealth;                      // Current health value

    public event Action OnDeath;                    // Event when health reaches zero
    public event Action<int, int> OnHealthChanged;  // Event when health changes (current, max)

    private bool isDead = false;                    // Flag to avoid double calls after death

    private void Awake()
    {
        currentHealth = maxHealth;                  // Set current health to max at start
    }

    /// <summary>
    /// מוריד חיים מהאובייקט
    /// </summary>
    /// <param name="damageAmount">כמות נזק</param>
    /// <returns>true אם נגרם נזק אמיתי, false אם לא</returns>
    public bool TakeDamage(int damageAmount)
    {
        if (isDead || damageAmount <= 0) return false;  // לא נגרם נזק

        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        if (currentHealth == 0)
        {
            isDead = true;
            OnDeath?.Invoke();
            Destroy(gameObject);
        }

        return true; // נזק בוצע בהצלחה
    }

    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}
