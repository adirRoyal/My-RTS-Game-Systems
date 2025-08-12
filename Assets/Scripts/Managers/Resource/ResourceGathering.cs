using UnityEngine;
using System.Collections;

public class ResourceGathering : MonoBehaviour
{
    [SerializeField] private int gatherRate = 10;           // How much resource gathered each time
    [SerializeField] private float gatherInterval = 1.5f;   // Time between gather actions
    [SerializeField] private float gatherDistance = 2f;     // Max distance from resource to start gathering

    private UnitMovement unitMovement;
    private ResourceNode targetResource;

    // Coroutines for arrival check and gathering loop
    private Coroutine gatherCoroutine;
    private Coroutine arrivalCheckCoroutine;
    private bool isGathering = false;

    private void Awake()
    {
        // Cache reference to UnitMovement component for movement commands
        unitMovement = GetComponent<UnitMovement>();
    }

    // Starts gathering from a given resource node
    public void StartGathering(ResourceNode resourceNode)
    {
        // If already gathering, stop previous gathering process before starting new one
        StopGathering();

        targetResource = resourceNode;
        MoveToResource();
    }

    // Sends the unit to the resource position and starts checking arrival
    private void MoveToResource()
    {
        if (targetResource != null)
        {
            unitMovement.MoveTo(targetResource.transform.position);
            arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
        }
    }

    // Coroutine: waits until unit is close enough to resource, then starts gathering
    private IEnumerator CheckArrivalAndGather()
    {
        while (!isGathering)
        {
            if (targetResource == null)
            {
                // Resource destroyed or null — stop gathering
                StopGathering();
                yield break;
            }

            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            if (distance <= gatherDistance)
            {
                isGathering = true;
                gatherCoroutine = StartCoroutine(GatherRoutine());
                yield break; // Exit arrival checking coroutine once gathering starts
            }

            yield return null;
        }
    }

    // Coroutine: gathers resource repeatedly at intervals until resource depleted or gathering stopped
    private IEnumerator GatherRoutine()
    {
        while (isGathering && targetResource != null)
        {
            // Gather from resource and add to global resource manager
            int gatheredAmount = targetResource.Gather(gatherRate);

            GameManager.Instance.ResourceManager.AddResource(targetResource.resourceType, gatheredAmount);

            yield return new WaitForSeconds(gatherInterval);

            if (targetResource.amount <= 0)
            {
                // Resource depleted — stop gathering
                StopGathering();
                yield break;
            }
        }
    }

    // Stops all gathering-related coroutines and clears target/resource state
    public void StopGathering()
    {
        if (arrivalCheckCoroutine != null)
        {
            StopCoroutine(arrivalCheckCoroutine);
            arrivalCheckCoroutine = null;
        }

        if (gatherCoroutine != null)
        {
            StopCoroutine(gatherCoroutine);
            gatherCoroutine = null;
        }

        isGathering = false;
        targetResource = null;
    }

    // IMPORTANT: override MoveTo to stop gathering if unit is moved manually by player or command
    public void MoveTo(Vector3 position)
    {
        StopGathering();
        unitMovement.MoveTo(position);
    }
}
