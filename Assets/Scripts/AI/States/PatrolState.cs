using UnityEngine;
using UnityEngine.AI;

public class PatrolState : IAIState
{
    private AIController ai;
    private Vector3 patrolTarget;

    public PatrolState(AIController ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.Agent.isStopped = false;
        patrolTarget = GetRandomPatrolPoint();
        ai.Agent.SetDestination(patrolTarget);
    }

    public void Update()
    {
        if (ai.IsBusy) return;
        // גילוי אויבים
        var target = FindNearestUnit();
        if (target != null)
        {
            ai.SetTarget(target);
            ai.SwitchState(new ChasingState(ai));
            return;
        }

        // סיים הליכה ? חזור לאיידל
        if (!ai.Agent.pathPending && ai.Agent.remainingDistance <= ai.StopDistance)
        {
            ai.SwitchState(new IdleState(ai));
        }
    }

    public void Exit() { }

    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * ai.PatrolRadius + ai.transform.position;
        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, ai.PatrolRadius, NavMesh.AllAreas))
            return hit.position;

        return ai.transform.position;
    }

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
        return best;
    }
}
