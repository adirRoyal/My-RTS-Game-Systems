using UnityEngine;
using System;

public class AttackSystem : MonoBehaviour
{
    [SerializeField] private float attackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private int damageAmount = 10;
    [SerializeField] private LayerMask enemyLayer;
    [SerializeField] private Transform attackOrigin;
    [SerializeField] private HitVisualEffect hitVisualEffect;

    private float lastAttackTime;

    public event Action OnAttack;

    private void Update()
    {
        if (Time.time >= lastAttackTime + attackCooldown)
        {
            TryAttack();
        }
    }

    private void TryAttack()
    {
        Collider[] enemiesInRange = Physics.OverlapSphere(attackOrigin.position, attackRange, enemyLayer);

        foreach (Collider enemy in enemiesInRange)
        {
            if (enemy.TryGetComponent(out HealthSystem enemyHealth))
            {
                hitVisualEffect.PlayEffect(enemy.transform.position + Vector3.up * 1f); // טיפה מעל האויב
                enemyHealth.TakeDamage(damageAmount);
                lastAttackTime = Time.time;
                OnAttack?.Invoke();
                break;
            }
        }
    }
}
