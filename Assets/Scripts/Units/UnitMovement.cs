using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles navigation-based movement for a unit using Unity's NavMesh system.
/// This component requires a NavMeshAgent and provides a simple API for setting destinations.
/// Designed for RTS-style or point-and-click movement.
/// </summary>
[RequireComponent(typeof(NavMeshAgent))]
public class UnitMovement : MonoBehaviour
{
    // Reference to the unit's NavMeshAgent used for pathfinding and movement.
    private NavMeshAgent agent;

    private void Awake()
    {
        // Cache the NavMeshAgent component for performance and cleaner access.
        agent = GetComponent<NavMeshAgent>();
    }

    /// <summary>
    /// Moves the unit to a specific world position using NavMesh pathfinding.
    /// </summary>
    /// <param name="destination">The target position in world space.</param>
    public void MoveTo(Vector3 destination)
    {
        // Set the desired destination; the NavMeshAgent handles path calculation and movement.
        agent.SetDestination(destination);
    }
}
