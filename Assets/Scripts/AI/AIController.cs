using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
public class AIController : MonoBehaviour
{
    // Added Gathering state
    public enum AIState { Idle, Patrol, Chasing, Gathering }

    [Header("Settings")]
    [SerializeField] private AIState currentState = AIState.Idle;
    [SerializeField] private float detectionRange = 10f;
    [SerializeField] private LayerMask unitLayer;
    [SerializeField] private float patrolRadius = 5f;
    [SerializeField] private float idleMinTime = 4f;
    [SerializeField] private float idleMaxTime = 10f;
    [SerializeField] private float stopDistance = 1f;

    private NavMeshAgent agent;
    private UnitMovement unitMovement;

    private Vector3 patrolTarget;
    private Transform targetUnit;

    private float idleTimer;
    private float currentIdleDuration;

    private void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        agent.stoppingDistance = stopDistance;
        unitMovement = GetComponent<UnitMovement>();

        StartIdle();
    }

    private void Update()
    {
        if (unitMovement != null && unitMovement.IsUnderPlayerControl) return;

        // Don't run detection/logic while gathering
        if (currentState == AIState.Gathering) return;

        FindNearestUnit();

        switch (currentState)
        {
            case AIState.Idle: HandleIdle(); break;
            case AIState.Patrol: HandlePatrol(); break;
            case AIState.Chasing: HandleChasing(); break;
        }
    }

    #region AI Logic
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

        if (targetUnit != null && currentState != AIState.Chasing)
            currentState = AIState.Chasing;
        else if (targetUnit == null && currentState == AIState.Chasing)
            StartIdle();
    }

    private void HandleIdle()
    {
        agent.isStopped = true;
        idleTimer += Time.deltaTime;

        if (idleTimer >= currentIdleDuration)
            StartPatrol();
    }

    private void HandlePatrol()
    {
        agent.isStopped = false;

        if (!agent.hasPath || agent.remainingDistance <= stopDistance)
            StartIdle();
    }

    private void HandleChasing()
    {
        if (targetUnit == null)
        {
            StartIdle();
            return;
        }

        agent.isStopped = false;
        agent.SetDestination(targetUnit.position);
    }
    #endregion

    #region State Transitions
    public void StartIdle()
    {
        currentState = AIState.Idle;
        idleTimer = 0f;
        currentIdleDuration = Random.Range(idleMinTime, idleMaxTime);
        agent.isStopped = true;
    }

    private void StartPatrol()
    {
        currentState = AIState.Patrol;
        patrolTarget = GetRandomPatrolPoint();
        agent.SetDestination(patrolTarget);
        agent.isStopped = false;
    }

    // === New Gathering State ===
    public void StartGathering()
    {
        currentState = AIState.Gathering;
        agent.isStopped = true; // stay in place
    }

    public void StopGathering()
    {
        StartIdle(); // after gathering, go back to idle AI cycle
    }
    #endregion

    private Vector3 GetRandomPatrolPoint()
    {
        Vector3 randomDir = Random.insideUnitSphere * patrolRadius + transform.position;

        if (NavMesh.SamplePosition(randomDir, out NavMeshHit hit, patrolRadius, NavMesh.AllAreas))
            return hit.position;

        return transform.position;
    }
}
