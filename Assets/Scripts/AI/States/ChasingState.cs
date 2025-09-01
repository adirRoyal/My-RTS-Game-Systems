using UnityEngine;

public class ChasingState : IAIState
{
    private AIController ai;

    public ChasingState(AIController ai)
    {
        this.ai = ai;
    }

    public void Enter()
    {
        ai.Agent.isStopped = false;
    }

    public void Update()
    {
        if (ai.TargetUnit == null)
        {
            ai.SwitchState(new IdleState(ai));
            return;
        }

        ai.Agent.SetDestination(ai.TargetUnit.position);

        // אם האויב ברח מחוץ לטווח – חזור לאיידל
        float dist = Vector3.Distance(ai.transform.position, ai.TargetUnit.position);
        if (dist > ai.DetectionRange)
        {
            ai.SetTarget(null);
            ai.SwitchState(new IdleState(ai));
        }
    }

    public void Exit() { }
}
