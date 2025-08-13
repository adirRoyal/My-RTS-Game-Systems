// ================= UnitMovement.cs =================
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    // Flag indicating if this unit is currently under player control (true) or AI control (false)
    public bool IsUnderPlayerControl { get; private set; }

    // Target position set by player commands
    public Vector3 TargetPosition { get; private set; }

    private NavMeshAgent agent;       // Reference to the NavMeshAgent for pathfinding
    private AIController aiController; // Reference to AIController to resume AI logic after player command

    private void Awake()
    {
        // Cache references for performance
        agent = GetComponent<NavMeshAgent>();
        aiController = GetComponent<AIController>();
    }

    private void Update()
    {
        // If unit is not under player control, AI is handling movement, so skip
        if (!IsUnderPlayerControl) return;

        // Check if agent has finished path and reached target reliably
        if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
        {
            // Player command completed
            IsUnderPlayerControl = false;

            // Notify AIController to resume its normal state behavior
            aiController?.StartIdle();
        }
    }

    /// <summary>
    /// Command the unit to move to a specific destination.
    /// </summary>
    /// <param name="destination">Target position in world space</param>
    /// <param name="isPlayerCommand">True if this move is from player input, false if AI</param>
    public void MoveTo(Vector3 destination, bool isPlayerCommand = true)
    {
        TargetPosition = destination;
        IsUnderPlayerControl = isPlayerCommand;

        // Reset the current path before issuing new destination
        agent.ResetPath();

        // Set the NavMeshAgent destination
        agent.SetDestination(destination);

        // Ensure the agent is not stopped
        agent.isStopped = false;
    }
}
