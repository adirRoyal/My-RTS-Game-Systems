using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;   // Maximum possible health
    private int currentHealth;                      // Current health value

    // Events
    public event Action OnDeath;                    // Invoked when health reaches zero
    public event Action<int, int> OnHealthChanged;  // Invoked whenever health changes (current, max)

    private bool isDead = false;                    // Prevents triggering death logic multiple times

    private void Awake()
    {
        // At start, entity begins at full health
        currentHealth = maxHealth;
    }

    /// <summary>
    /// Explicitly initialize or reset health system with a given max value.
    /// Useful for units/buildings with dynamic health stats.
    /// </summary>
    public void Initialize(int maxHealthValue)
    {
        maxHealth = maxHealthValue;
        currentHealth = maxHealth;
        isDead = false;

        // Notify listeners immediately of the reset health values
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    /// <summary>
    /// Apply damage to the entity.
    /// </summary>
    /// <param name="damageAmount">Amount of damage to apply.</param>
    /// <returns>
    /// true if damage was applied,
    /// false if damage was ignored (entity already dead or invalid input).
    /// </returns>
    public bool TakeDamage(int damageAmount)
    {
        // Ignore invalid damage values or attacks on a dead entity
        if (isDead || damageAmount <= 0)
            return false;

        // Reduce health and clamp to zero
        currentHealth -= damageAmount;
        currentHealth = Mathf.Max(currentHealth, 0);

        // Notify UI/other systems of updated health
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        // Handle death
        if (currentHealth == 0)
        {
            isDead = true;
            OnDeath?.Invoke();

            // Destroy the GameObject (could be adjusted to disable instead,
            // depending on pooling/revive systems).
            Destroy(gameObject);
        }

        return true;
    }

    // --- Accessors ---
    public int GetCurrentHealth() => currentHealth;
    public int GetMaxHealth() => maxHealth;
    public bool IsDead() => isDead;
}
