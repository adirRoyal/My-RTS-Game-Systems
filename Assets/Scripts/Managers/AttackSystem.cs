using UnityEngine;
using System;

/// <summary>
/// Handles melee or close-range attacks for a unit or player.
/// Uses a cooldown and deals damage to enemies within range.
/// </summary>
public class AttackSystem : MonoBehaviour
{
    [Header("Attack Settings")]
    [SerializeField] private float attackRange = 2f;         // How far the attack can reach
    [SerializeField] private float attackCooldown = 1f;      // Time between attacks
    [SerializeField] private int damageAmount = 10;          // How much damage we deal

    [Header("Targeting")]
    [SerializeField] private LayerMask enemyLayer;           // Which layers are considered enemies
    [SerializeField] private Transform attackOrigin;         // Where the attack originates (usually player)

    [Header("Visual Effects")]
    [SerializeField] private HitVisualEffect hitVisualEffect;// Play effect on hit

    // Track last attack time
    private float lastAttackTime;

    // Pre-allocated buffer for performance (avoids GC allocations)
    private Collider[] enemiesBuffer = new Collider[10];

    // Event fired when attack happens
    public event Action OnAttack;

    private void Update()
    {
        // Check if cooldown finished
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttack();
        }
    }

    /// <summary>
    /// Attempts to attack enemies within range.
    /// </summary>
    private void TryAttack()
    {
        // Check for colliders in attack range without allocating memory
        int hits = Physics.OverlapSphereNonAlloc(
            attackOrigin.position,
            attackRange,
            enemiesBuffer,
            enemyLayer
        );

        for (int i = 0; i < hits; i++)
        {
            Collider enemy = enemiesBuffer[i];

            // Check if collider has HealthSystem
            if (enemy.TryGetComponent(out HealthSystem enemyHealth))
            {
                // Deal damage
                bool tookDamage = enemyHealth.TakeDamage(damageAmount);

                // Play hit effect if damage was applied
                if (tookDamage && hitVisualEffect != null)
                {
                    hitVisualEffect.PlayEffect(enemy.transform.position + Vector3.up);
                }

                // Update last attack time
                lastAttackTime = Time.time;

                // Fire attack event
                OnAttack?.Invoke();

                // Only attack one enemy per cooldown
                break;
            }
        }
    }
}
