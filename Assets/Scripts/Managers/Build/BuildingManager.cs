using System.Collections.Generic;
using UnityEngine;

public class BuildingManager : MonoBehaviour
{
    // Singleton instance for easy global access
    public static BuildingManager Instance { get; private set; }

    [Header("All Buildings")]
    [SerializeField] private List<BuildingData> allBuildings = new List<BuildingData>();
    // Serialized list to hold all building types (set in inspector)

    // Internal dictionary for fast lookup by building name
    private Dictionary<string, BuildingData> buildingDictionary;

    private void Awake()
    {
        // --- Singleton pattern ---
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // --- Initialize dictionary for fast building lookup ---
        buildingDictionary = new Dictionary<string, BuildingData>();
        foreach (var building in allBuildings)
        {
            if (!buildingDictionary.ContainsKey(building.buildingName))
            {
                buildingDictionary.Add(building.buildingName, building);
            }
            else
            {
                // Warn if duplicate building names exist
                Debug.LogWarning($"Building {building.buildingName} is duplicated!");
            }
        }
    }

    /// <summary>
    /// Get a building by its name. Returns null if not found.
    /// </summary>
    public BuildingData GetBuildingByName(string name)
    {
        if (buildingDictionary.TryGetValue(name, out var data))
            return data;

        Debug.LogWarning($"Building {name} not found!");
        return null;
    }

    /// <summary>
    /// Returns the full list of all buildings.
    /// </summary>
    public List<BuildingData> GetAllBuildings()
    {
        return allBuildings;
    }

    // --- Optional Improvements ---
    // 1. You could add a method to add/remove buildings at runtime.
    // 2. Could validate building names in editor to avoid duplicates automatically.
    // 3. Consider using ScriptableObject references for more modularity.
}
