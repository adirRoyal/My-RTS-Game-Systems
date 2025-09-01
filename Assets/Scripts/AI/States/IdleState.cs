using UnityEngine;

public class IdleState : IAIState
{
    private AIController ai;
    private float idleTimer;
    private float idleDuration;

    public IdleState(AIController ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.Agent.isStopped = true;
        idleTimer = 0f;
        idleDuration = Random.Range(ai.IdleMinTime, ai.IdleMaxTime);
    }

    public void Update()
    {
        if (ai.IsBusy) return;
        idleTimer += Time.deltaTime;

        // גילוי אויבים
        var target = FindNearestUnit();
        if (target != null)
        {
            ai.SetTarget(target);
            ai.SwitchState(new ChasingState(ai));
            return;
        }

        // מעבר לפטרול אחרי זמן
        if (idleTimer >= idleDuration)
        {
            ai.SwitchState(new PatrolState(ai));
        }
    }

    public void Exit() { }

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
