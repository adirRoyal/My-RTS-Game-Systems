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
    private Collider[] enemiesBuffer = new Collider[10]; // buffer לחיסכון בהקצאה

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
        int hits = Physics.OverlapSphereNonAlloc(attackOrigin.position, attackRange, enemiesBuffer, enemyLayer);

        for (int i = 0; i < hits; i++)
        {
            Collider enemy = enemiesBuffer[i];
            if (enemy.TryGetComponent(out HealthSystem enemyHealth))
            {
                hitVisualEffect.PlayEffect(enemy.transform.position + Vector3.up);
                enemyHealth.TakeDamage(damageAmount);

                lastAttackTime = Time.time;
                OnAttack?.Invoke();
                break; // תקוף אויב אחד בלבד
            }

        }
    }
}
