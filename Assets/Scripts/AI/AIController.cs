// ================= AIController.cs =================
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    // Enum representing the different AI states
    public enum AIState { Idle, Patrol, Chasing }

    [Header("Settings")]
    [SerializeField] private AIState currentState = AIState.Idle; // Current AI state
    [SerializeField] private float detectionRange = 10f;           // Range to detect nearby units
    [SerializeField] private LayerMask unitLayer;                  // Layer for units to detect
    [SerializeField] private float patrolRadius = 5f;              // Radius around unit to pick random patrol points
    [SerializeField] private float idleMinTime = 4f;               // Minimum idle duration
    [SerializeField] private float idleMaxTime = 10f;              // Maximum idle duration
    [SerializeField] private float stopDistance = 1f;             // Stopping distance for agent

    private NavMeshAgent agent;          // NavMeshAgent component for movement
    private UnitMovement unitMovement;    // Reference to UnitMovement to detect player control

    private Vector3 patrolTarget;         // Current patrol destination
    private Transform targetUnit;         // Target unit for chasing

    private float idleTimer;              // Timer counting how long the unit has been idle
    private float currentIdleDuration;    // Randomized idle duration for current idle session

    private void Awake()
    {
        // Cache components for performance
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        unitMovement = GetComponent<UnitMovement>();

        // Start AI in Idle state
        StartIdle();
    }

    private void Update()
    {
        // Skip AI logic if the player is currently controlling this unit
        if (unitMovement != null && unitMovement.IsUnderPlayerControl) return;

        // Regular AI behavior
        FindNearestUnit();

        // Call state-specific logic
        switch (currentState)
        {
            case AIState.Idle: HandleIdle(); break;
            case AIState.Patrol: HandlePatrol(); break;
            case AIState.Chasing: HandleChasing(); break;
        }
    }

    #region AI Logic
    // Check for nearest unit in range and update targetUnit
    private void FindNearestUnit()
    {
        Collider[] unitsInRange = Physics.OverlapSphere(transform.position, detectionRange, unitLayer);
        float closestDistance = Mathf.Infinity;
        Transform closestUnit = null;

        foreach (var unitCollider in unitsInRange)
        {
            float dist = Vector3.Distance(transform.position, unitCollider.transform.position);
            if (dist < closestDistance)
            {
                closestDistance = dist;
                closestUnit = unitCollider.transform;
            }
        }

        targetUnit = closestUnit;

        // If a target is found, switch to Chasing state
        if (targetUnit != null && currentState != AIState.Chasing)
            currentState = AIState.Chasing;
        // If target lost during chase, go back to Idle
        else if (targetUnit == null && currentState == AIState.Chasing)
            StartIdle();
    }

    private void HandleIdle()
    {
        // Stop the agent while idle
        agent.isStopped = true;

        // Count elapsed idle time
        idleTimer += Time.deltaTime;

        // After idle duration, switch to patrol
        if (idleTimer >= currentIdleDuration)
            StartPatrol();
    }

    private void HandlePatrol()
    {
        // Make sure agent is moving
        agent.isStopped = false;

        // If reached patrol point or path is invalid, go back to idle
        if (!agent.hasPath || agent.remainingDistance <= stopDistance)
            StartIdle();
    }

    private void HandleChasing()
    {
        // If target lost, return to idle
        if (targetUnit == null)
        {
            StartIdle();
            return;
        }

        // Move towards the target unit
        agent.isStopped = false;
        agent.SetDestination(targetUnit.position);
    }
    #endregion

    #region State Transitions
    // Public because UnitMovement needs to call this to resume AI after player command
    public void StartIdle()
    {
        currentState = AIState.Idle;
        idleTimer = 0f;
        currentIdleDuration = Random.Range(idleMinTime, idleMaxTime);

        // Stop agent during idle
        agent.isStopped = true;
    }

    // Private since only AI internally triggers patrol
    private void StartPatrol()
    {
        currentState = AIState.Patrol;

        // Pick a random point for patrol
        patrolTarget = GetRandomPatrolPoint();
        agent.SetDestination(patrolTarget);

        // Ensure agent is moving
        agent.isStopped = false;
    }
    #endregion

    // Generate a random valid point on NavMesh for patrol
    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            return hit.position;

        // If no valid point found, stay in place
        return transform.position;
    }
}
