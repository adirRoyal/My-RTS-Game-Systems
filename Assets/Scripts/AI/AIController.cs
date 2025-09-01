using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// AIController handles the AI logic for a unit:
/// - Switches between states (Idle, Patrol, Chasing, Gathering)
/// - Manages target detection and busy status
/// - Interfaces with NavMeshAgent for movement
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float detectionRange = 10f; // how far AI can see enemies
    [SerializeField] private LayerMask unitLayer;       // which layers AI considers as units
    [SerializeField] private float patrolRadius = 5f;   // radius for random patrol points
    [SerializeField] private float idleMinTime = 4f;    // min idle duration
    [SerializeField] private float idleMaxTime = 10f;   // max idle duration
    [SerializeField] private float stopDistance = 1f;   // stopping distance for NavMeshAgent

    private NavMeshAgent agent;         // reference to NavMeshAgent
    private UnitMovement unitMovement;  // reference to UnitMovement (player control)
    private IAIState currentState;      // current AI state

    // --- Public properties ---
    public Transform TargetUnit { get; private set; }      // current target unit
    public bool IsBusy { get; private set; } = false;     // true when AI is performing action like gathering
    public float DetectionRange => detectionRange;
    public LayerMask UnitLayer => unitLayer;
    public float PatrolRadius => patrolRadius;
    public float IdleMinTime => idleMinTime;
    public float IdleMaxTime => idleMaxTime;
    public float StopDistance => stopDistance;
    public NavMeshAgent Agent => agent;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        unitMovement = GetComponent<UnitMovement>();

        agent.stoppingDistance = stopDistance; // configure agent stopping distance

        // start AI in idle state
        SwitchState(new IdleState(this));
    }

    private void Update()
    {
        // skip AI update if unit is under player control
        if (unitMovement != null && unitMovement.IsUnderPlayerControl) return;

        // update current AI state
        currentState?.Update();
    }

    /// <summary>
    /// Switches AI to a new state
    /// </summary>
    public void SwitchState(IAIState newState)
    {
        currentState?.Exit(); // exit previous state
        currentState = newState;
        currentState.Enter(); // enter new state
    }

    /// <summary>
    /// Sets a target for the AI (usually an enemy unit)
    /// </summary>
    public void SetTarget(Transform target)
    {
        TargetUnit = target;
    }

    // === Public API for other components ===

    /// <summary>
    /// Start gathering action (sets busy and switches to GatheringState)
    /// </summary>
    public void StartGathering()
    {
        IsBusy = true;
        SwitchState(new GatheringState(this));
    }

    /// <summary>
    /// Stop gathering action (sets free and switches to IdleState)
    /// </summary>
    public void StopGathering()
    {
        IsBusy = false;
        SwitchState(new IdleState(this));
    }

    /// <summary>
    /// Forces AI into idle state
    /// </summary>
    public void StartIdle()
    {
        SwitchState(new IdleState(this));
    }
}
