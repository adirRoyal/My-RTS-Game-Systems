using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

/// <summary>
/// Handles player building placement: ghost preview, placement validation, resource & supply checks.
/// Uses InputManager (new Unity Input System) instead of old Input.
/// </summary>
public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera; // Camera used for raycasts
    [SerializeField] private LayerMask groundMask;   // Ground layers allowed for placement
    [SerializeField] private LayerMask obstacleMask; // Layers that block placement (buildings, units, etc)

    [Header("UI")]
    [SerializeField] private GameMessageUI messageUI; // UI messages like "not enough resources"

    private ResourceManager resourceManager;
    private BuildingData selectedBuilding;   // Building selected for placement
    private GameObject ghostInstance;        // Transparent preview object
    private GhostVisualizer ghostVisualizer; // Changes color depending on placement validity

    private Vector2 lastPointerScreenPos;    // Last pointer position on screen
    private Vector3 lastPointerWorldPos;     // Last pointer position projected to world

    private void Start()
    {
        resourceManager = GameManager.Instance.ResourceManager;

        // --- Subscribe to InputManager events ---
        InputManager.OnPointerPositionChanged += HandlePointerMoved;
        InputManager.OnLeftClick += HandleLeftClick;
        InputManager.OnRightClick += HandleRightClick;
        InputManager.OnExitPressed += HandleExitPressed;
    }

    private void OnDestroy()
    {
        // --- Unsubscribe to avoid memory leaks ---
        InputManager.OnPointerPositionChanged -= HandlePointerMoved;
        InputManager.OnLeftClick -= HandleLeftClick;
        InputManager.OnRightClick -= HandleRightClick;
        InputManager.OnExitPressed -= HandleExitPressed;
    }

    // Called whenever mouse moves
    private void HandlePointerMoved(Vector2 screenPos)
    {
        lastPointerScreenPos = screenPos;

        if (selectedBuilding == null || ghostInstance == null) return; // no building, do nothing

        // Raycast to ground
        Ray ray = mainCamera.ScreenPointToRay(screenPos);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            lastPointerWorldPos = hit.point;
            ghostInstance.transform.position = hit.point; // move ghost to mouse position

            // Check placement validity
            bool isValid = IsValidPlacement(hit.point);

            if (ghostVisualizer != null)
            {
                if (isValid) ghostVisualizer.SetValid();   // green if valid
                else ghostVisualizer.SetInvalid();         // red if invalid
            }
        }
    }

    // Called on left click
    private void HandleLeftClick(Vector2 screenPos)
    {
        if (selectedBuilding == null || ghostInstance == null) return;

        // Only place if valid
        if (IsValidPlacement(lastPointerWorldPos))
        {
            PlaceBuilding(lastPointerWorldPos);
        }
    }

    // Called on right click
    private void HandleRightClick(Vector2 screenPos)
    {
        CancelPlacement(); // cancel placement on right click
    }

    // Start placing a new building
    public void StartPlacement(BuildingData buildingData)
    {
        CancelPlacement(); // remove old ghost

        selectedBuilding = buildingData;
        ghostInstance = Instantiate(buildingData.ghostPrefab); // create ghost
        ghostVisualizer = ghostInstance.GetComponent<GhostVisualizer>();

        if (ghostVisualizer == null)
            Debug.LogWarning("Ghost prefab missing GhostVisualizer!");
    }

    // Actually place building in world
    // Actually place building in world
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

        // --- Instantiate construction site (parent) ---
        GameObject constructionSite = new GameObject("ConstructionSite_" + selectedBuilding.buildingName);
        constructionSite.transform.position = position; // חשוב! שנייצב אותו במקום הנכון

        // --- Compute obstacle size/center from prefab collider (scaled) ---
        Vector3 obstSize = new Vector3(5, 5, 5);
        Vector3 obstCenter = Vector3.zero;

        BoxCollider prefabCollider = selectedBuilding.prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Vector3 scaledSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
            Vector3 scaledCenter = Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

            // תוספת מרווח קטן כדי ש־Agents לא "יידבקו" לגבול
            const float padding = 0.5f;
            obstSize = new Vector3(scaledSize.x + padding, scaledSize.y, scaledSize.z + padding);
            obstCenter = scaledCenter;
        }

        // --- Add NavMeshObstacle on the ConstructionSite (stationary carving) ---
        var siteObstacle = constructionSite.AddComponent<NavMeshObstacle>();
        siteObstacle.shape = NavMeshObstacleShape.Box;
        siteObstacle.carving = true;
        siteObstacle.carveOnlyStationary = true;
        siteObstacle.carvingMoveThreshold = 0.1f;
        siteObstacle.carvingTimeToStationary = 0.1f;
        siteObstacle.size = obstSize;
        siteObstacle.center = obstCenter;

        // --- Add construction logic (passes obstacle so it can be transferred on finalize) ---
        var construction = constructionSite.AddComponent<BuildingConstruction>();
        construction.Initialize(selectedBuilding, position, siteObstacle);

        CancelPlacement(); // remove ghost and reset state
    }


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

    // Cancel placement, remove ghost
    private void CancelPlacement()
    {
        if (ghostInstance != null)
            Destroy(ghostInstance);

        ghostInstance = null;
        ghostVisualizer = null;
        selectedBuilding = null;
    }

    // Check if position is valid for building
    private bool IsValidPlacement(Vector3 position)
    {
        if (selectedBuilding == null) return false;

        BoxCollider prefabCollider = selectedBuilding.prefab.GetComponent<BoxCollider>();
        if (prefabCollider == null)
        {
            Debug.LogError("Prefab of " + selectedBuilding.buildingName + " missing BoxCollider!");
            return false;
        }

        // calculate world size and center of collider
        Vector3 worldSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
        Vector3 halfExtents = worldSize * 0.5f;
        Vector3 center = position + Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

        // check for collisions with obstacles
        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, obstacleMask);

        foreach (Collider hit in hits)
        {
            // ignore ghost itself
            if (ghostInstance != null && hit.transform.IsChildOf(ghostInstance.transform))
                continue;

            return false; // hit something ? invalid
        }

        return true; // placement valid
    }

    // ESC pressed ? cancel
    private void HandleExitPressed()
    {
        CancelPlacement();
    }
}
