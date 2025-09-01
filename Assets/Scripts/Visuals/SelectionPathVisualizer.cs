using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class SelectionPathVisualizer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab;
    [SerializeField] private GameObject endPointPrefab;
    [SerializeField] private float pointYOffset = 0.05f;

    private UnitSelectionHandler selectionHandler;

    private readonly Dictionary<UnitMovement, LineRenderer> activeLines = new();
    private readonly Dictionary<UnitMovement, GameObject> activeEndPoints = new();

    private void Awake()
    {
        selectionHandler = FindFirstObjectByType<UnitSelectionHandler>();
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick += UpdateAllPaths;
            Selectable.OnSelected += OnUnitSelected;
            Selectable.OnDeselected += OnUnitDeselected;
        }
    }

    private void OnDestroy()
    {
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick -= UpdateAllPaths;
            Selectable.OnSelected -= OnUnitSelected;
            Selectable.OnDeselected -= OnUnitDeselected;
        }
    }

    private void OnUnitSelected(Selectable selectable)
    {
        if (!selectable.TryGetComponent<UnitMovement>(out var mover))
            return;

        // אם היחידה כבר בתנועה, צור את הקווים מיד
        if (mover.IsUnderPlayerControl)
            CreateLineAndMarker(mover);

        // הירשם לאירוע פקודת תנועה
        mover.OnMoveCommandIssued += () => CreateLineAndMarker(mover);
    }

    private void CreateLineAndMarker(UnitMovement mover)
    {
        if (activeLines.ContainsKey(mover))
            return;

        GameObject lineObj = PoolManager.Instance.GetFromPool(linePrefab);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        activeLines[mover] = lr;

        GameObject endPoint = PoolManager.Instance.GetFromPool(endPointPrefab);
        activeEndPoints[mover] = endPoint;

        // הירשם גם לאירוע סיום הגעה
        mover.OnReachedDestination += () => Cleanup(mover);

        // הירשם לאירוע השמדה של היחידה
        mover.OnDestroyed += () => Cleanup(mover);
    }

    private void OnUnitDeselected(Selectable selectable)
    {
        if (selectable == null || selectable.gameObject == null)
            return;

        if (!selectable.TryGetComponent<UnitMovement>(out var mover) || mover == null)
            return;

        Cleanup(mover);
    }

    private void UpdateAllPaths(Vector3 targetPosition)
    {
        foreach (var pair in activeLines)
        {
            UnitMovement unit = pair.Key;
            LineRenderer lr = pair.Value;

            if (unit == null || lr == null) continue;

            NavMeshPath path = new();
            unit.GetComponent<NavMeshAgent>().CalculatePath(targetPosition, path);

            lr.positionCount = path.corners.Length;
            lr.SetPositions(path.corners);

            if (activeEndPoints.TryGetValue(unit, out var endPoint) && endPoint != null)
                endPoint.transform.position = targetPosition + Vector3.up * pointYOffset;
        }
    }

    private void Update()
    {
        foreach (var pair in activeLines)
        {
            UnitMovement unit = pair.Key;
            LineRenderer lr = pair.Value;

            if (unit == null || lr == null || !unit.IsUnderPlayerControl) continue;

            NavMeshAgent agent = unit.GetComponent<NavMeshAgent>();
            if (!agent.hasPath) continue;

            lr.positionCount = agent.path.corners.Length;
            lr.SetPositions(agent.path.corners);

            if (activeEndPoints.TryGetValue(unit, out var endPoint) && endPoint != null)
                endPoint.transform.position = unit.TargetPosition + Vector3.up * pointYOffset;
        }
    }

    private void Cleanup(UnitMovement mover)
    {
        if (activeLines.TryGetValue(mover, out var lr))
        {
            if (lr != null)
                PoolManager.Instance.ReturnToPool(linePrefab, lr.gameObject);
            activeLines.Remove(mover);
        }

        if (activeEndPoints.TryGetValue(mover, out var endPoint))
        {
            if (endPoint != null)
                PoolManager.Instance.ReturnToPool(endPointPrefab, endPoint);
            activeEndPoints.Remove(mover);
        }
    }
}
