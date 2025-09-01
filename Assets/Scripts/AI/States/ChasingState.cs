using UnityEngine;

/// <summary>
/// AI state for chasing a target unit.
/// The AI will follow the target until it's out of detection range or null.
/// </summary>
public class ChasingState : IAIState
{
    private AIController ai;

    public ChasingState(AIController ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// Called when AI enters chasing state
    /// Ensures the NavMeshAgent is moving.
    /// </summary>
    public void Enter()
    {
        // Make sure AI can move
        ai.Agent.isStopped = false;

        // Optional: trigger chase animation
        // ai.Animator.SetTrigger("Chase");
    }

    /// <summary>
    /// Called every frame while in chasing state
    /// Moves the AI towards the target unit and checks distance.
    /// </summary>
    public void Update()
    {
        // If target is null (destroyed or lost), switch back to Idle
        if (ai.TargetUnit == null)
        {
            ai.SwitchState(new IdleState(ai));
            return;
        }

        // Move towards target unit
        ai.Agent.SetDestination(ai.TargetUnit.position);

        // Check if target is outside detection range
        float distance = Vector3.Distance(ai.transform.position, ai.TargetUnit.position);
        if (distance > ai.DetectionRange)
        {
            // Stop chasing and switch to Idle
            ai.SetTarget(null);
            ai.SwitchState(new IdleState(ai));
        }
    }

    /// <summary>
    /// Called when exiting chasing state
    /// Can reset animations or stop effects here.
    /// </summary>
    public void Exit()
    {
        // Optional: reset chase animation
        // ai.Animator.ResetTrigger("Chase");
    }
}
