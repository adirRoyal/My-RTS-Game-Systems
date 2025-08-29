using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Handles player building placement: ghost preview, placement validation, resource & supply checks.
/// </summary>
public class BuildingPlacementSystem : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private Camera mainCamera; // Main camera used for raycasting
    [SerializeField] private LayerMask groundMask;   // Ground layers allowed for placement
    [SerializeField] private LayerMask obstacleMask; // Layers that block placement (buildings, units, obstacles)

    [Header("UI")]
    [SerializeField] private GameMessageUI messageUI; // UI for displaying messages like errors

    private ResourceManager resourceManager;
    private BuildingData selectedBuilding;   // The building currently selected for placement
    private GameObject ghostInstance;        // Transparent "ghost" prefab preview
    private GhostVisualizer ghostVisualizer; // Controls ghost color (valid/invalid placement)

    private void Start()
    {
        resourceManager = GameManager.Instance.ResourceManager;
    }

    private void Update()
    {
        if (selectedBuilding == null || ghostInstance == null) return;

        // --- Raycast from mouse to ground ---
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        if (Physics.Raycast(ray, out RaycastHit hit, 1000f, groundMask))
        {
            ghostInstance.transform.position = hit.point;

            // --- Check if placement is valid ---
            bool isValid = IsValidPlacement(hit.point);

            if (ghostVisualizer != null)
            {
                if (isValid)
                    ghostVisualizer.SetValid();
                else
                    ghostVisualizer.SetInvalid();
            }

            // --- Place building on left click if valid ---
            if (isValid && Input.GetMouseButtonDown(0))
            {
                PlaceBuilding(hit.point);
            }
        }

        // --- Cancel placement on right click or Escape ---
        if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
        {
            CancelPlacement();
        }
    }

    /// <summary>
    /// Starts placement of a building: spawns ghost and stores selection.
    /// </summary>
    public void StartPlacement(BuildingData buildingData)
    {
        CancelPlacement(); // Reset any previous placement

        selectedBuilding = buildingData;
        ghostInstance = Instantiate(buildingData.ghostPrefab);
        ghostVisualizer = ghostInstance.GetComponent<GhostVisualizer>();

        if (ghostVisualizer == null)
        {
            Debug.LogWarning("Ghost prefab missing GhostVisualizer!");
        }
    }

    /// <summary>
    /// Attempts to place the building at a valid position.
    /// Checks resources, supply, and creates a construction site.
    /// </summary>
    private void PlaceBuilding(Vector3 position)
    {
        if (selectedBuilding == null) return;

        // --- Required resources ---
        var required = new Dictionary<ResourceType, int>
        {
            { ResourceType.Gold, selectedBuilding.costGold },
            { ResourceType.Wood, selectedBuilding.costWood }
        };

        // --- Check resources ---
        if (!resourceManager.HasEnoughResources(required))
        {
            string msg = BuildResourceErrorMessage(selectedBuilding, required);
            messageUI.ShowMessage(msg);
            Debug.Log(msg);
            return;
        }

        // --- Check population/supply ---
        if (!resourceManager.HasFreeSupply(selectedBuilding.requiredPopulation))
        {
            string msg = $"{selectedBuilding.buildingName}: Not enough supply! " +
                         $"Need {selectedBuilding.requiredPopulation}, " +
                         $"Current {resourceManager.CurrentSupply}/{resourceManager.MaxSupply}";
            messageUI.ShowMessage(msg);
            Debug.Log(msg);
            return;
        }

        // --- Consume resources and supply ---
        resourceManager.ConsumeResources(required);
        resourceManager.ConsumeSupply(selectedBuilding.requiredPopulation);

        // --- Create construction site ---
        GameObject constructionSite = new GameObject("ConstructionSite_" + selectedBuilding.buildingName);
        var construction = constructionSite.AddComponent<BuildingConstruction>();
        construction.Initialize(selectedBuilding, position);

        // --- Add population cap if applicable ---
        if (selectedBuilding.providesPopulation)
        {
            resourceManager.AddSupplyCap(selectedBuilding.populationProvided);
        }

        CancelPlacement();
    }

    /// <summary>
    /// Builds a detailed error message for missing resources.
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
    /// Cancels current building placement and destroys ghost.
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
    /// Checks if the placement position is valid (no obstacles, units, or other buildings).
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

        // Calculate world size of prefab collider
        Vector3 worldSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
        Vector3 halfExtents = worldSize * 0.5f;

        // Center position of the collider
        Vector3 center = position + Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

        Collider[] hits = Physics.OverlapBox(center, halfExtents, Quaternion.identity, obstacleMask);

        foreach (Collider hit in hits)
        {
            if (ghostInstance != null && hit.transform.IsChildOf(ghostInstance.transform))
                continue;

            return false; // Collision detected
        }

        return true;
    }

    /// <summary>
    /// Draws Gizmos in editor for debugging placement area.
    /// </summary>
    private void OnDrawGizmos()
    {
        if (selectedBuilding != null && ghostInstance != null)
        {
            BoxCollider prefabCollider = selectedBuilding.prefab.GetComponent<BoxCollider>();
            if (prefabCollider != null)
            {
                Gizmos.color = Color.red;

                Vector3 worldSize = Vector3.Scale(prefabCollider.size, selectedBuilding.prefab.transform.localScale);
                Vector3 center = ghostInstance.transform.position + Vector3.Scale(prefabCollider.center, selectedBuilding.prefab.transform.localScale);

                Gizmos.DrawWireCube(center, worldSize);
            }
        }
    }

    // --- Optional Improvements ---
    // 1. Use object pooling for ghostInstance instead of instantiating/destroying each time.
    // 2. Smooth movement of ghostInstance instead of snapping to hit.point.
    // 3. Add snapping to grid based on footprintSize from BuildingData.
    // 4. Extend IsValidPlacement to check terrain slope, water, or other gameplay rules.
}
