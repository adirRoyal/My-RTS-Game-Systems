using UnityEngine;
using System;

public class AttackSystem : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f;         // How far the attack can reach
    [SerializeField] private float attackCooldown = 1f;      // Time between attacks
    [SerializeField] private int damageAmount = 10;           // How much damage we do
    [SerializeField] private LayerMask enemyLayer;            // Which layer enemies are on
    [SerializeField] private Transform attackOrigin;          // Position where attack starts (usually player)
    [SerializeField] private HitVisualEffect hitVisualEffect; // Effect to show when hitting enemy

    private float lastAttackTime;                             // When we last attacked

    public event Action OnAttack;                             // Event called when attack happens

    private void Update()
    {
        // Check if cooldown is over and we can attack again
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttack();  // Try to attack enemies in range
        }
    }

    private void TryAttack()
    {
        // Find all enemies inside the attack range sphere
        Collider[] enemiesInRange = Physics.OverlapSphere(attackOrigin.position, attackRange, enemyLayer);

        // Go through all enemies found
        foreach (Collider enemy in enemiesInRange)
        {
            // Check if this enemy has a HealthSystem component
            if (enemy.TryGetComponent(out HealthSystem enemyHealth))
            {
                // Play hit effect a little above enemy
                hitVisualEffect.PlayEffect(enemy.transform.position + Vector3.up * 1f);

                // Make enemy take damage
                enemyHealth.TakeDamage(damageAmount);

                // Update last attack time so cooldown works
                lastAttackTime = Time.time;

                // Tell listeners that attack happened
                OnAttack?.Invoke();

                break;  // Attack only one enemy per attack
            }
        }
    }
}
