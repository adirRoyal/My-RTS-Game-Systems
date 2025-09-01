using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Patrol state for AI units:
/// - Moves randomly within PatrolRadius
/// - Scans for enemy units
/// - Switches to ChasingState if enemy detected
/// - Returns to IdleState when reaching patrol target
/// </summary>
public class PatrolState : IAIState
{
    private AIController ai;        // Reference to AI controller
    private Vector3 patrolTarget;   // Current patrol destination

    public PatrolState(AIController ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// Called when entering PatrolState
    /// </summary>
    public void Enter()
    {
        ai.Agent.isStopped = false;              // Make sure NavMeshAgent is moving
        patrolTarget = GetRandomPatrolPoint();   // Pick random patrol target
        ai.Agent.SetDestination(patrolTarget);   // Move towards it
    }

    /// <summary>
    /// Called every frame while in PatrolState
    /// </summary>
    public void Update()
    {
        if (ai.IsBusy) return; // Skip update if AI is busy (e.g., gathering or attacking)

        // --- Look for enemies in detection range ---
        var target = FindNearestUnit();
        if (target != null)
        {
            ai.SetTarget(target);
            ai.SwitchState(new ChasingState(ai)); // Switch to chasing if enemy found
            return;
        }

        // --- Check if reached patrol destination ---
        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= ai.StopDistance)
        {
            ai.SwitchState(new IdleState(ai)); // Go idle when done patrolling
        }
    }

    public void Exit()
    {
        // Nothing special to do on exit
    }

    /// <summary>
    /// Picks a random point on the NavMesh within patrol radius
    /// </summary>
    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * ai.PatrolRadius + ai.transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, ai.PatrolRadius, NavMesh.AllAreas))
            return hit.position;

        return ai.transform.position; // fallback to current position if sampling fails
    }

    /// <summary>
    /// Finds the nearest unit within detection range
    /// </summary>
    private Transform FindNearestUnit()
    {
        Collider[] units = Physics.OverlapSphere(ai.transform.position, ai.DetectionRange, ai.UnitLayer);
        float closest = Mathf.Infinity;
        Transform best = null;

        foreach (var unit in units)
        {
            float dist = Vector3.Distance(ai.transform.position, unit.transform.position);
            if (dist < closest)
            {
                closest = dist;
                best = unit.transform;
            }
        }

        return best; // Returns null if no units found
    }
}
