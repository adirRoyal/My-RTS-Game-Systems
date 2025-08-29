using UnityEngine;

public class BuildMenuUI : MonoBehaviour
{
    [Header("UI Setup")]
    [SerializeField] private GameObject buttonPrefab; // Prefab for each building button
    [SerializeField] private Transform buttonContainer; // Parent transform to hold all buttons

    [Header("Systems References")]
    [SerializeField] private BuildingPlacementSystem placementSystem; // Reference to the placement system

    private BuildingManager buildingManager; // Cached reference to BuildingManager

    private void Start()
    {
        // --- Find BuildingManager in the scene ---
        buildingManager = FindFirstObjectByType<BuildingManager>();

        if (buildingManager == null)
        {
            Debug.LogError("? BuildingManager not found in scene!");
            return;
        }

        // --- Ensure PlacementSystem is assigned ---
        if (placementSystem == null)
        {
            Debug.LogError("? PlacementSystem reference missing in BuildMenuUI!");
            return;
        }

        // --- Generate buttons for all buildings ---
        GenerateButtons();
    }

    /// <summary>
    /// Instantiates a button for each building and sets it up.
    /// </summary>
    private void GenerateButtons()
    {
        foreach (BuildingData data in buildingManager.GetAllBuildings())
        {
            GameObject buttonObj = Instantiate(buttonPrefab, buttonContainer);
            BuildingButtonUI buttonUI = buttonObj.GetComponent<BuildingButtonUI>();

            if (buttonUI == null)
            {
                Debug.LogError("? Button prefab missing BuildingButtonUI component!");
                continue;
            }

            // --- Initialize button with building data and PlacementSystem reference ---
            buttonUI.Setup(data, placementSystem);
        }
    }

    // --- Optional Improvements ---
    // 1. Pool buttons to reduce instantiation cost if menu is regenerated frequently.
    // 2. Add a ClearButtons method to remove old buttons before regenerating.
    // 3. Consider sorting buttons by building type or name for better UI organization.
}
