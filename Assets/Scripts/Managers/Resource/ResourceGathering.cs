using UnityEngine;
using System.Collections;

/// <summary>
/// Handles unit resource gathering with integration to AIController and GameManager.
/// </summary>
public class ResourceGathering : MonoBehaviour
{
    [Header("Gathering Settings")]
    [SerializeField] private int gatherRate = 10;
    [SerializeField] private float gatherInterval = 1.5f;
    [SerializeField] private float gatherDistance = 2f;

    private UnitMovement unitMovement;
    private ResourceNode targetResource;
    private AIController aiController;

    private Coroutine gatherCoroutine;
    private Coroutine arrivalCheckCoroutine;
    private bool isGathering = false;

    private void Awake()
    {
        unitMovement = GetComponent<UnitMovement>();
        aiController = GetComponent<AIController>();
    }

    /// <summary>
    /// Starts moving to a resource node and begins gathering when in range.
    /// </summary>
    public void StartGathering(ResourceNode resourceNode)
    {
        StopGathering();
        targetResource = resourceNode;
        MoveToResource();
    }

    private void MoveToResource()
    {
        if (targetResource != null)
        {
            unitMovement.MoveTo(targetResource.transform.position);
            arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
        }
    }

    private IEnumerator CheckArrivalAndGather()
    {
        while (!isGathering)
        {
            if (targetResource == null)
            {
                StopGathering();
                yield break;
            }

            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            if (distance <= gatherDistance)
            {
                isGathering = true;
                aiController?.StartGathering(); // Notify AI we started gathering
                gatherCoroutine = StartCoroutine(GatherRoutine());
                yield break;
            }

            yield return null;
        }
    }

    private IEnumerator GatherRoutine()
    {
        while (isGathering && targetResource != null)
        {
            int gatheredAmount = targetResource.Gather(gatherRate);
            GameManager.Instance.ResourceManager.AddResource(targetResource.resourceType, gatheredAmount);

            yield return new WaitForSeconds(gatherInterval);

            if (targetResource.amount <= 0)
            {
                StopGathering();
                yield break;
            }
        }
    }

    /// <summary>
    /// Stops all gathering activity and resets state.
    /// </summary>
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

        aiController?.StopGathering(); // Notify AI to go back to idle
    }

    /// <summary>
    /// Command the unit to move somewhere manually, canceling gathering.
    /// </summary>
    public void MoveTo(Vector3 position)
    {
        StopGathering();
        unitMovement.MoveTo(position);
    }
}
