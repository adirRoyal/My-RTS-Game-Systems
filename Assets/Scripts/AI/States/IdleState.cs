using UnityEngine;

/// <summary>
/// Represents the AI "Idle" state within a state machine.
/// While idle, the AI agent stands still for a random duration,
/// periodically checking for enemies within its detection range.
/// If an enemy is detected, it transitions to a "Chasing" state.
/// If no enemy is detected after the idle duration, it transitions to a "Patrol" state.
/// </summary>
public class IdleState : IAIState
{
    // --- References ---
    private AIController ai;
    // Reference to the AI controller that owns this state.
    // Provides access to navigation, detection ranges, and state switching.

    // --- Timers ---
    private float idleTimer;
    // Tracks how long the AI has been idle so far.

    private float idleDuration;
    // Randomized idle duration chosen upon entering this state.

    /// <summary>
    /// Constructor requires a reference to the owning AIController.
    /// </summary>
    public IdleState(AIController ai)
    {
        this.ai = ai;
    }

    /// <summary>
    /// Called when the AI enters the Idle state.
    /// Stops movement and initializes a random idle duration.
    /// </summary>
    public void Enter()
    {
        ai.Agent.isStopped = true;
        // Prevents the NavMeshAgent from moving.

        idleTimer = 0f;
        // Reset the timer to start counting fresh.

        // Choose a random idle time between configured min/max values.
        idleDuration = Random.Range(ai.IdleMinTime, ai.IdleMaxTime);
    }

    /// <summary>
    /// Called every frame while in the Idle state.
    /// Handles enemy detection and state transitions.
    /// </summary>
    public void Update()
    {
        // If the AI is performing another action (e.g., attacking or busy),
        // we do not update idle logic.
        if (ai.IsBusy) return;

        // Increment idle timer.
        idleTimer += Time.deltaTime;

        // --- Enemy Detection ---
        var target = FindNearestUnit();
        if (target != null)
        {
            // If an enemy is found, switch to Chasing state immediately.
            ai.SetTarget(target);
            ai.SwitchState(new ChasingState(ai));
            return;
        }

        // --- Patrol Transition ---
        // If idle time has expired and no enemy was detected,
        // switch to patrol behavior.
        if (idleTimer >= idleDuration)
        {
            ai.SwitchState(new PatrolState(ai));
        }
    }

    /// <summary>
    /// Called when exiting the Idle state.
    /// No cleanup is needed here for now.
    /// </summary>
    public void Exit() { }

    /// <summary>
    /// Finds the nearest unit within detection range.
    /// Uses Physics.OverlapSphere to detect colliders on the UnitLayer.
    /// Returns the Transform of the closest detected unit, or null if none.
    /// </summary>
    private Transform FindNearestUnit()
    {
        // Collect all colliders representing potential units in range.
        Collider[] units = Physics.OverlapSphere(ai.transform.position, ai.DetectionRange, ai.UnitLayer);

        float closest = Mathf.Infinity;
        // Keeps track of the shortest distance found.

        Transform best = null;
        // Stores the best candidate unit.

        // Loop through all detected units to find the closest one.
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
