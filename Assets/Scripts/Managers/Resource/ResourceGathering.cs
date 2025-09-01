using System.Collections;
using UnityEngine;

public class ResourceGathering : MonoBehaviour
{
    [Header("Gathering Settings")]
    [SerializeField] private int gatherRate = 10;
    [SerializeField] private float gatherInterval = 1.5f;
    [SerializeField] private float gatherDistance = 2f;
    [SerializeField] private float resourceSearchRadius = 15f;

    private UnitMovement unitMovement;
    private ResourceNode targetResource;
    private AIController aiController;

    private Coroutine gatherCoroutine;
    private Coroutine arrivalCheckCoroutine;
    private bool isGathering = false;
    private bool isMovingToResource = false;

    private void Awake()
    {
        unitMovement = GetComponent<UnitMovement>();
        aiController = GetComponent<AIController>();
    }

    public void StartGathering(ResourceNode resourceNode)
    {
        if (resourceNode == null) return;

        StopGathering(); // עצור פעולות קודמות

        targetResource = resourceNode;
        targetResource.OnDepleted += OnResourceDepleted;

        isMovingToResource = true;
        unitMovement.MoveTo(targetResource.transform.position);

        arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
    }

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
                isMovingToResource = false;
                isGathering = true;

                // Switch AI to GatheringState
                aiController?.SwitchState(new GatheringState(aiController));

                gatherCoroutine = StartCoroutine(GatherRoutine());
                yield break;
            }

            yield return null;
        }
    }


    private IEnumerator GatherRoutine()
    {
        while (isGathering)
        {
            if (targetResource == null)
            {
                StopGathering();
                yield break;
            }

            // אם היחידה נדחקה מהמשאב – תחזור אליו
            float distance = Vector3.Distance(transform.position, targetResource.transform.position);
            if (distance > gatherDistance)
            {
                // עצור איסוף, חזור למשאב
                isGathering = false;
                isMovingToResource = true;
                unitMovement.MoveTo(targetResource.transform.position);
                arrivalCheckCoroutine = StartCoroutine(CheckArrivalAndGather());
                yield break;
            }

            // בדיקה אם המשאב נגמר
            if (targetResource.amount <= 0)
            {
                OnResourceDepleted();
                yield break;
            }

            // איסוף בפועל
            ResourceType type = targetResource.resourceType;
            int gatheredAmount = targetResource.Gather(gatherRate);

            if (GameManager.Instance?.ResourceManager != null)
                GameManager.Instance.ResourceManager.AddResource(type, gatheredAmount);

            yield return new WaitForSeconds(gatherInterval);
        }
    }



    private void OnResourceDepleted()
    {
        if (targetResource != null)
            targetResource.OnDepleted -= OnResourceDepleted;

        ResourceType depletedType = targetResource != null ? targetResource.resourceType : ResourceType.Wood;

        // עצירה מוחלטת של כל תהליכי האיסוף לפני המשך
        StopGathering();

        ResourceNode next = FindClosestResourceOfType(depletedType);
        if (next != null)
        {
            // להמתין פריים אחד כדי שהקורוטינות הקודמות ייסגרו
            StartCoroutine(RestartGathering(next));
        }
    }

    private IEnumerator RestartGathering(ResourceNode next)
    {
        yield return null; // מחכה פריים אחד
        StartGathering(next);
    }


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

        // חזור למצב Idle / Patrol
        aiController?.SwitchState(new IdleState(aiController));
    }


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


    public void MoveTo(Vector3 position)
    {
        StopGathering();
        unitMovement.MoveTo(position);
    }
}
