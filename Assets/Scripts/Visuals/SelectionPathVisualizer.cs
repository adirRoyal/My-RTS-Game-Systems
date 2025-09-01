using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// This script draws a path (LineRenderer) from selected units to their target destination,
/// and places a marker at the endpoint. Works with units controlled by the player.
/// </summary>
public class SelectionPathVisualizer : MonoBehaviour
{
    [Header("Line Settings")]
    [SerializeField] private GameObject linePrefab;       // prefab with LineRenderer component
    [SerializeField] private GameObject endPointPrefab;   // prefab for destination marker
    [SerializeField] private float pointYOffset = 0.05f;  // small offset to make marker float above ground

    private UnitSelectionHandler selectionHandler;

    // Dictionaries to keep track of active lines and endpoints per unit
    private readonly Dictionary<UnitMovement, LineRenderer> activeLines = new();
    private readonly Dictionary<UnitMovement, GameObject> activeEndPoints = new();

    private void Awake()
    {
        // find the selection handler in the scene
        selectionHandler = FindFirstObjectByType<UnitSelectionHandler>();
        if (selectionHandler != null)
        {
            // subscribe to events
            selectionHandler.OnGroundClick += UpdateAllPaths;    // update paths when player clicks ground
            Selectable.OnSelected += OnUnitSelected;            // create path for newly selected unit
            Selectable.OnDeselected += OnUnitDeselected;        // remove path when unit is deselected
        }
    }

    private void OnDestroy()
    {
        // unsubscribe from events to avoid memory leaks
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick -= UpdateAllPaths;
            Selectable.OnSelected -= OnUnitSelected;
            Selectable.OnDeselected -= OnUnitDeselected;
        }
    }

    private void OnUnitSelected(Selectable selectable)
    {
        // only work with units that have UnitMovement
        if (!selectable.TryGetComponent<UnitMovement>(out var mover))
            return;

        // if the unit is under player control and already moving, immediately create a path
        if (mover.IsUnderPlayerControl)
            CreateLineAndMarker(mover);

        // subscribe to movement command event
        mover.OnMoveCommandIssued += () => CreateLineAndMarker(mover);
    }

    private void CreateLineAndMarker(UnitMovement mover)
    {
        if (activeLines.ContainsKey(mover))
            return; // already has a line

        // get line from pool
        GameObject lineObj = PoolManager.Instance.GetFromPool(linePrefab);
        LineRenderer lr = lineObj.GetComponent<LineRenderer>();
        activeLines[mover] = lr;

        // get endpoint marker from pool
        GameObject endPoint = PoolManager.Instance.GetFromPool(endPointPrefab);
        activeEndPoints[mover] = endPoint;

        // subscribe to events to cleanup when unit reaches destination or is destroyed
        mover.OnReachedDestination += () => Cleanup(mover);
        mover.OnDestroyed += () => Cleanup(mover);
    }

    private void OnUnitDeselected(Selectable selectable)
    {
        if (selectable == null || selectable.gameObject == null)
            return;

        if (!selectable.TryGetComponent<UnitMovement>(out var mover) || mover == null)
            return;

        Cleanup(mover); // remove the path and marker
    }

    private void UpdateAllPaths(Vector3 targetPosition)
    {
        // called when player clicks on the ground
        foreach (var pair in activeLines)
        {
            UnitMovement unit = pair.Key;
            LineRenderer lr = pair.Value;

            if (unit == null || lr == null) continue;

            // calculate path using NavMesh
            NavMeshPath path = new();
            unit.GetComponent<NavMeshAgent>().CalculatePath(targetPosition, path);

            lr.positionCount = path.corners.Length;
            lr.SetPositions(path.corners);

            // update endpoint marker position
            if (activeEndPoints.TryGetValue(unit, out var endPoint) && endPoint != null)
                endPoint.transform.position = targetPosition + Vector3.up * pointYOffset;
        }
    }

    private void Update()
    {
        // update all active paths every frame for moving units
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
        // remove line renderer
        if (activeLines.TryGetValue(mover, out var lr))
        {
            if (lr != null)
                PoolManager.Instance.ReturnToPool(linePrefab, lr.gameObject);
            activeLines.Remove(mover);
        }

        // remove endpoint marker
        if (activeEndPoints.TryGetValue(mover, out var endPoint))
        {
            if (endPoint != null)
                PoolManager.Instance.ReturnToPool(endPointPrefab, endPoint);
            activeEndPoints.Remove(mover);
        }
    }
}
