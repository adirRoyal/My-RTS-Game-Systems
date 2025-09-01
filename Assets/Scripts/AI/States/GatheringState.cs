using UnityEngine;

/// <summary>
/// AI state for gathering resources.
/// When in this state, the AI stops moving and performs gathering actions.
/// </summary>
public class GatheringState : IAIState
{
    private AIController ai;

    public GatheringState(AIController ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// Called when the AI enters the Gathering state
    /// Stops the NavMeshAgent so the unit stays in place while gathering.
    /// </summary>
    public void Enter()
    {
        // Stop the AI's movement
        ai.Agent.isStopped = true;


        // Optional: trigger animation for gathering here if you have an animator
        // ai.Animator.SetTrigger("Gather");
    }

    /// <summary>
    /// Called every frame while in Gathering state
    /// Currently empty because actual gathering is handled by ResourceGathering component.
    /// </summary>
    public void Update()
    {
        // If needed, could check for resource depletion or other events
        // For example, switch to Idle if resource is gone:
        // if (ai.TargetResource == null) ai.SwitchState(new IdleState(ai));
    }

    /// <summary>
    /// Called when exiting Gathering state
    /// Resets AI to not busy and prepares for next state.
    /// </summary>
    public void Exit()
    {
        // Allow AI to move again
        ai.Agent.isStopped = false;


        // Optional: reset gathering animation
        // ai.Animator.ResetTrigger("Gather");
    }
}
