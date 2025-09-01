using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float idleMinTime = 4f;
    [SerializeField] private float idleMaxTime = 10f;
    [SerializeField] private float stopDistance = 1f;

    private NavMeshAgent agent;
    private UnitMovement unitMovement;
    private IAIState currentState;

    public Transform TargetUnit { get; private set; }
    public bool IsBusy { get; private set; } = false;
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

        agent.stoppingDistance = stopDistance;

        SwitchState(new IdleState(this));
    }

    private void Update()
    {
        if (unitMovement != null && unitMovement.IsUnderPlayerControl) return;

        currentState?.Update();
    }

    public void SwitchState(IAIState newState)
    {
        currentState?.Exit();
        currentState = newState;
        currentState.Enter();
    }

    public void SetTarget(Transform target)
    {
        TargetUnit = target;
    }

    // === Public API for other components ===
    public void StartGathering()
    {
        IsBusy = true;
        SwitchState(new GatheringState(this));
    }

    public void StopGathering()
    {
        IsBusy = false;
        SwitchState(new IdleState(this));
    }


    public void StartIdle()
    {
        SwitchState(new IdleState(this));
    }
}
