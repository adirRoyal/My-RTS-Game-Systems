using UnityEngine;

/// <summary>
/// Component attached to building GameObjects to manage data, health, and world health bar.
/// Works similarly to the Unit script.
/// </summary>
[RequireComponent(typeof(HealthSystem))] // חובה על הבניין HealthSystem
public class BuildingComponent : MonoBehaviour
{
    [Header("Building Data")]
    [SerializeField] private BuildingData buildingData;
    public BuildingData Data => buildingData;

    [Header("UI")]
    [SerializeField] private HealthBar worldHealthBar; // Health bar above building

    private HealthSystem healthSystem;
    public HealthSystem Health => healthSystem;

    private void Awake()
    {
        // Initialize health system
        healthSystem = GetComponent<HealthSystem>();

        // אם רוצים, ניתן להוסיף Health מקורי מתוך BuildingData (למשל 500 ברירת מחדל)
        int initialHealth = buildingData != null ? 500 : 100;
        healthSystem.Initialize(initialHealth);

        // Link health bar UI
        if (worldHealthBar != null)
            worldHealthBar.SetHealthSystem(healthSystem);
    }
}
