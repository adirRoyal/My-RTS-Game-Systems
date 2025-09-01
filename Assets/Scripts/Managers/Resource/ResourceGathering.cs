using System.Collections;
using UnityEngine;

public class ResourceGathering : MonoBehaviour
{
    [Header("Gathering Settings")]
    [SerializeField] private int gatherRate = 10;              // Amount of resource gathered per cycle
    [SerializeField] private float gatherInterval = 1.5f;      // Time between each gather cycle
    [SerializeField] private float gatherDistance = 2f;        // Required distance from resource to gather
    [SerializeField] private float resourceSearchRadius = 15f; // Radius to look for new resources when depleted

    // Cached references
    private UnitMovement unitMovement;     // Handles movement of the unit
    private ResourceNode targetResource;   // Current resource node being gathered
    private AIController aiController;     // AI state controller for this unit

    // Coroutine handlers (so we can stop them cleanly)
    private Coroutine gatherCoroutine;
    private Coroutine arrivalCheckCoroutine;

    // State flags
    private bool isGathering = false;
    private bool isMovingToResource = false;

    private void Awake()
    {
        // Cache required components on the same GameObject
        unitMovement = GetComponent<UnitMovement>();
        aiController = GetComponent<AIController>();
    }

    /// <summary>
    /// Starts gathering from the specified resource node.
    /// Cancels any previous gathering operation.
    /// </summary>
    public void StartGathering(ResourceNode resourceNode)
    {
        if (resourceNode == null) return;

        // Stop any previous gathering before starting a new one
        StopGathering();

        targetResource = resourceNode;
        targetResource.OnDepleted += OnResourceDepleted; // Subscribe to depletion event

        isMovingToResource = true;
        unitMovement.MoveTo(targetResource.transform.position); // Move towards resource

        // Start monitoring arrival at resource node
        arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
    }

    /// <summary>
    /// Coroutine that checks if the unit has arrived near the target resource.
    /// If close enough, switches to gathering mode.
    /// </summary>
    private IEnumerator CheckArrivalAndGather()
    {
        while (isMovingToResource)
        {
            if (targetResource == null)
            {
                StopGathering();
                yield break;
            }

            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            if (distance <= gatherDistance)
            {
                // Arrived at resource node
                isMovingToResource = false;
                isGathering = true;

                // Switch AI to Gathering state (if applicable)
                aiController?.SwitchState(new GatheringState(aiController));

                // Start gathering loop
                gatherCoroutine = StartCoroutine(GatherRoutine());
                yield break;
            }

            yield return null; // wait next frame
        }
    }

    /// <summary>
    /// Coroutine that continuously gathers resources while conditions are met.
    /// Handles resource depletion and re-approach if pushed away.
    /// </summary>
    private IEnumerator GatherRoutine()
    {
        while (isGathering)
        {
            if (targetResource == null)
            {
                StopGathering();
                yield break;
            }

            // If the unit was pushed away, interrupt gathering and move back
            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            if (distance > gatherDistance)
            {
                isGathering = false;
                isMovingToResource = true;
                unitMovement.MoveTo(targetResource.transform.position);
                arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
                yield break;
            }

            // Stop if resource is empty
            if (targetResource.amount <= 0)
            {
                OnResourceDepleted();
                yield break;
            }

            // Perform actual gathering
            ResourceType type = targetResource.resourceType;
            int gatheredAmount = targetResource.Gather(gatherRate);

            // Add gathered resources to global ResourceManager (if available)
            if (GameManager.Instance?.ResourceManager != null)
                GameManager.Instance.ResourceManager.AddResource(type, gatheredAmount);

            yield return new WaitForSeconds(gatherInterval);
        }
    }

    /// <summary>
    /// Callback triggered when the resource node is depleted.
    /// Stops gathering and attempts to find a new resource of the same type.
    /// </summary>
    private void OnResourceDepleted()
    {
        if (targetResource != null)
            targetResource.OnDepleted -= OnResourceDepleted; // Unsubscribe from old node

        ResourceType depletedType = targetResource != null ? targetResource.resourceType : ResourceType.Wood;

        StopGathering(); // Ensure all gathering coroutines are stopped

        // Look for the next available resource of the same type
        ResourceNode next = FindClosestResourceOfType(depletedType);
        if (next != null)
        {
            // Delay by one frame to ensure coroutines from StopGathering() have finished
            StartCoroutine(RestartGathering(next));
        }
    }

    private IEnumerator RestartGathering(ResourceNode next)
    {
        yield return null; // wait 1 frame
        StartGathering(next);
    }

    /// <summary>
    /// Stops all gathering and resets internal state.
    /// Returns unit to Idle or Patrol state.
    /// </summary>
    private void StopGathering()
    {
        if (arrivalCheckCoroutine != null) StopCoroutine(arrivalCheckCoroutine);
        if (gatherCoroutine != null) StopCoroutine(gatherCoroutine);

        arrivalCheckCoroutine = null;
        gatherCoroutine = null;

        isGathering = false;
        isMovingToResource = false;

        if (targetResource != null)
            targetResource.OnDepleted -= OnResourceDepleted;

        targetResource = null;

        // Return to idle behavior when not gathering
        aiController?.SwitchState(new IdleState(aiController));
    }

    /// <summary>
    /// Finds the closest available resource node of the specified type.
    /// </summary>
    private ResourceNode FindClosestResourceOfType(ResourceType type)
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, resourceSearchRadius);
        ResourceNode closest = null;
        float closestDist = Mathf.Infinity;

        foreach (var hit in hits)
        {
            ResourceNode node = hit.GetComponent<ResourceNode>();
            if (node != null && node.resourceType == type && node.amount > 0)
            {
                float dist = Vector3.Distance(transform.position, node.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = node;
                }
            }
        }

        return closest;
    }

    /// <summary>
    /// Cancels gathering behavior and issues a direct move command.
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        StopGathering();
        unitMovement.MoveTo(position);
    }
}
