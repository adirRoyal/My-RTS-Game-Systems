using UnityEngine;
using System;

public class HealthSystem : MonoBehaviour
{
    [SerializeField] private int maxHealth = 100;   // Max health value
    private int currentHealth;                      // Current health value

    public event Action OnDeath;                    // Event when health reaches zero
    public event Action<int, int> OnHealthChanged; // Event when health changes (current, max)

    private bool isDead = false;                    // Flag to avoid double calls after death

    private void Awake()
    {
        currentHealth = maxHealth;                  // Set current health to max at start
    }

    public void TakeDamage(int damageAmount)
    {
        if (isDead) return;                         // If already dead, ignore damage

        currentHealth -= damageAmount;              // Reduce health by damage amount
        currentHealth = Mathf.Max(currentHealth, 0);// Prevent health from going below 0

        OnHealthChanged?.Invoke(currentHealth, maxHealth); // Notify listeners about health change

        if (currentHealth == 0)                     // If health is zero
        {
            isDead = true;                          // Mark as dead
            OnDeath?.Invoke();                      // Notify listeners about death

            Destroy(gameObject);                    // Destroy this game object
        }
    }

    public int GetCurrentHealth() => currentHealth; // Get current health value
    public int GetMaxHealth() => maxHealth;         // Get max health value
}
