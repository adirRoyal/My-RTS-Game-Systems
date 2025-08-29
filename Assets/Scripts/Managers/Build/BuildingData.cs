using UnityEngine;

[CreateAssetMenu(fileName = "BuildingData", menuName = "RTS/Building Data")]
public class BuildingData : ScriptableObject
{
    [Header("General Info")]
    public string buildingName;  // Name of the building, displayed in UI
    public Sprite icon;          // Icon to represent building in UI menus
    public GameObject prefab;    // Actual building prefab placed in the scene
    public GameObject ghostPrefab; // Transparent "preview" prefab shown before placement

    [Header("Construction Settings")]
    public float buildTime = 5f; // Time in seconds it takes to construct this building
    public int costGold = 100;   // Gold cost for constructing
    public int costWood = 0;     // Wood cost for constructing
    public int requiredPopulation = 0; // How many population slots are needed to build

    [Header("Gameplay")]
    public bool isResourceBuilding;   // Example: Gold Mine, used to gather resources
    public bool isMilitaryBuilding;   // Example: Barracks, produces units
    public bool providesPopulation;   // Example: Farm, increases max population
    public int populationProvided = 0; // How much population this building adds if providesPopulation is true

    [Header("Placement")]
    public Vector2 footprintSize = new Vector2(2, 2); // Approximate size on the grid for placement calculations
    public LayerMask placementMask; // Specifies which layers this building can be placed on

    // --- Optional: Helper Methods ---
    // You could add helper methods here in the future, like checking if placement is valid
}
