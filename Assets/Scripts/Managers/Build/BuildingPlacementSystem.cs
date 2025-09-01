using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Manages the player’s building placement system:
/// - Shows a ghost preview of the building at the cursor.
/// - Validates placement (ground, collisions, resources, supply).
/// - Handles confirmation (left click), cancellation (right click/ESC).
/// - Consumes resources and instantiates a construction site with a NavMeshObstacle.
/// 
/// This script integrates with:
/// - InputManager (new Unity Input System)
/// - ResourceManager (for resource & supply checks)
/// - GameMessageUI (for user feedback)
/// </summary>
public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera;
    // Camera used for raycasting from screen pointer to world.

    [SerializeField] private LayerMask groundMask;
    // Defines which layers count as "valid ground" for placement.

    [SerializeField] private LayerMask obstacleMask;
    // Defines which layers block placement (e.g., units, existing buildings).

    [Header("UI")]
    [SerializeField] private GameMessageUI messageUI;
    // Displays placement errors (not enough resources, supply, etc).

    // --- Core Systems ---
    private ResourceManager resourceManager; // Central resource tracker.
    private BuildingData selectedBuilding;   // Currently selected building type for placement.
    private GameObject ghostInstance;        // Transparent "ghost" version of the building for preview.
    private GhostVisualizer ghostVisualizer; // Responsible for coloring the ghost (valid = green, invalid = red).

    // --- Pointer State ---
    private Vector2 lastPointerScreenPos;    // Last pointer position in screen-space.
    private Vector3 lastPointerWorldPos;     // Last pointer position projected into the 3D world.

    private void Start()
    {
        // Get reference to ResourceManager from GameManager singleton.
        resourceManager = GameManager.Instance.ResourceManager;

        // Subscribe to input events from InputManager (new Input System).
        InputManager.OnPointerPositionChanged += HandlePointerMoved;
        InputManager.OnLeftClick += HandleLeftClick;
        InputManager.OnRightClick += HandleRightClick;
        InputManager.OnExitPressed += HandleExitPressed;
    }

    private void OnDestroy()
    {
        // Unsubscribe from events to prevent memory leaks or invalid callbacks.
        InputManager.OnPointerPositionChanged -= HandlePointerMoved;
        InputManager.OnLeftClick -= HandleLeftClick;
        InputManager.OnRightClick -= HandleRightClick;
        InputManager.OnExitPressed -= HandleExitPressed;
    }

    /// <summary>
    /// Called whenever the mouse moves.
    /// Updates the ghost preview position and colors it based on validity.
    /// </summary>
    private void HandlePointerMoved(Vector2 screenPos)
    {
        lastPointerScreenPos = screenPos;

        // If no building is selected or ghost not instantiated, do nothing.
        if (selectedBuilding == null || ghostInstance == null) return;

        // Raycast from camera to ground to position the ghost.
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            lastPointerWorldPos = hit.point;
            ghostInstance.transform.position = hit.point;

            // Validate placement and update ghost visuals.
            bool isValid = IsValidPlacement(hit.point);

            if (ghostVisualizer != null)
            {
                if (isValid) ghostVisualizer.SetValid();
                else ghostVisualizer.SetInvalid();
            }
        }
    }

    /// <summary>
    /// Called on left click.
    /// Confirms placement if valid.
    /// </summary>
    private void HandleLeftClick(Vector2 screenPos)
    {
        if (selectedBuilding == null || ghostInstance == null) return;

        if (IsValidPlacement(lastPointerWorldPos))
        {
            PlaceBuilding(lastPointerWorldPos);
        }
    }

    /// <summary>
    /// Called on right click.
    /// Cancels current placement.
    /// </summary>
    private void HandleRightClick(Vector2 screenPos)
    {
        CancelPlacement();
    }

    /// <summary>
    /// Begins the placement process for a given building.
    /// Spawns a ghost preview instance.
    /// </summary>
    public void StartPlacement(BuildingData buildingData)
    {
        CancelPlacement(); // Ensure old ghost is cleared.

        selectedBuilding = buildingData;
        ghostInstance = Instantiate(buildingData.ghostPrefab);
        ghostVisualizer = ghostInstance.GetComponent<GhostVisualizer>();

        if (ghostVisualizer == null)
            Debug.LogWarning("Ghost prefab missing GhostVisualizer!");
    }

    /// <summary>
    /// Finalizes placement: consumes resources/supply, spawns construction site,
    /// and places a NavMeshObstacle to block unit pathing.
    /// </summary>
    private void PlaceBuilding(Vector3 position)
    {
        if (selectedBuilding == null) return;

        // --- Check resources ---
        var required = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, selectedBuilding.costGold },
            { ResourceType.Wood, selectedBuilding.costWood }
        };

        if (!resourceManager.HasEnoughResources(required))
        {
            string msg = BuildResourceErrorMessage(selectedBuilding, required);
            messageUI.ShowMessage(msg);
            Debug.Log(msg);
            return;
        }

        // --- Check supply ---
        if (!resourceManager.HasFreeSupply(selectedBuilding.requiredPopulation))
        {
            string msg = $"{selectedBuilding.buildingName}: Not enough supply! " +
                         $"Need {selectedBuilding.requiredPopulation}, " +
                         $"Current {resourceManager.CurrentSupply}/{resourceManager.MaxSupply}";
            messageUI.ShowMessage(msg);
            Debug.Log(msg);
            return;
        }

        // --- Consume resources & supply ---
        resourceManager.ConsumeResources(required);
        resourceManager.ConsumeSupply(selectedBuilding.requiredPopulation);

        // --- Create parent object for construction ---
        GameObject constructionSite = new GameObject("ConstructionSite_" + selectedBuilding.buildingName);
        constructionSite.transform.position = position;

        // --- Compute NavMeshObstacle size from prefab collider ---
        Vector3 obstSize = new Vector3(5, 5, 5);
        Vector3 obstCenter = Vector3.zero;

        BoxCollider prefabCollider = selectedBuilding.prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Vector3 scaledSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
            Vector3 scaledCenter = Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

            // Small padding so agents don’t "stick" to the building edges.
            const float padding = 0.5f;
            obstSize = new Vector3(scaledSize.x + padding, scaledSize.y, scaledSize.z + padding);
            obstCenter = scaledCenter;
        }

        // --- Add NavMeshObstacle to block unit pathing while building exists ---
        var siteObstacle = constructionSite.AddComponent<NavMeshObstacle>();
        siteObstacle.shape = NavMeshObstacleShape.Box;
        siteObstacle.carving = true;                // Dynamically carves NavMesh
        siteObstacle.carveOnlyStationary = true;    // Only carves when not moving
        siteObstacle.carvingMoveThreshold = 0.1f;
        siteObstacle.carvingTimeToStationary = 0.1f;
        siteObstacle.size = obstSize;
        siteObstacle.center = obstCenter;

        // --- Add construction logic ---
        var construction = constructionSite.AddComponent<BuildingConstruction>();
        construction.Initialize(selectedBuilding, position, siteObstacle);

        CancelPlacement(); // Remove ghost after placing.
    }

    /// <summary>
    /// Builds a detailed error message when resources are insufficient.
    /// </summary>
    private string BuildResourceErrorMessage(BuildingData building, Dictionary<ResourceType, int> required)
    {
        System.Text.StringBuilder sb = new System.Text.StringBuilder();
        sb.Append($"{building.buildingName}: Not enough resources!\n");

        foreach (var pair in required)
        {
            int requiredAmount = pair.Value;
            int currentAmount = resourceManager.GetAmount(pair.Key);

            if (requiredAmount > 0 && currentAmount < requiredAmount)
            {
                int missing = requiredAmount - currentAmount;
                sb.AppendLine($"- {pair.Key}: Need {requiredAmount}, Have {currentAmount} (Missing {missing})");
            }
        }

        return sb.ToString();
    }

    /// <summary>
    /// Cancels placement and destroys the ghost preview.
    /// </summary>
    private void CancelPlacement()
    {
        if (ghostInstance != null)
            Destroy(ghostInstance);

        ghostInstance = null;
        ghostVisualizer = null;
        selectedBuilding = null;
    }

    /// <summary>
    /// Validates if a given position is valid for placement.
    /// Uses prefab collider bounds to check against obstacleMask with Physics.OverlapBox.
    /// </summary>
    private bool IsValidPlacement(Vector3 position)
    {
        if (selectedBuilding == null) return false;

        BoxCollider prefabCollider = selectedBuilding.prefab.GetComponent<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("Prefab of " + selectedBuilding.buildingName + " missing BoxCollider!");
            return false;
        }

        // Compute world-space size and position of collider.
        Vector3 worldSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
        Vector3 halfExtents = worldSize * 0.5f;
        Vector3 center = position + Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

        // Check if overlaps any obstacles.
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, obstacleMask);

        foreach (Collider hit in hits)
        {
            // Ignore collisions with the ghost itself.
            if (ghostInstance != null && hit.transform.IsChildOf(ghostInstance.transform))
                continue;

            return false; // Hit something invalid.
        }

        return true; // No collisions, placement valid.
    }

    /// <summary>
    /// ESC pressed handler ? cancels current placement.
    /// </summary>
    private void HandleExitPressed()
    {
        CancelPlacement();
    }
}
