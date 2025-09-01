using UnityEngine;

/// <summary>
/// Component attached to Building GameObjects to manage building data, health, and world health bar.
/// Works similarly to the Unit script for consistency.
/// </summary>
[RequireComponent(typeof(HealthSystem))] // Ensure every building has a HealthSystem
public class BuildingComponent : MonoBehaviour
{
    [Header("Building Data")]
    [SerializeField] private BuildingData buildingData; // ScriptableObject with building info
    public BuildingData Data => buildingData; // Public getter for other scripts

    [Header("UI")]
    [SerializeField] private HealthBar worldHealthBar; // Health bar displayed above the building in the world

    private HealthSystem healthSystem; // Reference to the HealthSystem component
    public HealthSystem Health => healthSystem; // Public getter for external access

    private void Awake()
    {
        // --- Initialize HealthSystem ---
        healthSystem = GetComponent<HealthSystem>();

        // Optional: set initial health based on BuildingData or default to 500
        int initialHealth = buildingData != null ? 500 : 100;
        healthSystem.Initialize(initialHealth);

        // --- Link the world health bar if assigned ---
        if (worldHealthBar != null)
        {
            worldHealthBar.SetHealthSystem(healthSystem);
        }
    }
}
