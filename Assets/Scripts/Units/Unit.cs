using UnityEngine;

/// <summary>
/// Represents a Unit in the game, holds data and health, and connects to UI health bars.
/// </summary>
[RequireComponent(typeof(HealthSystem))] // Ensure HealthSystem exists on this GameObject
public class Unit : MonoBehaviour
{
    [Header("Unit Data")]
    [SerializeField] private UnitData unitData;        // Reference to ScriptableObject containing unit info

    [Header("UI")]
    [SerializeField] private HealthBar worldHealthBar; // Health bar above unit in the world (World Canvas)

    private HealthSystem healthSystem;                 // Internal reference to unit's HealthSystem

    // Public getters for external access
    public UnitData Data => unitData;
    public HealthSystem Health => healthSystem;

    private void Awake()
    {
        // --- Initialize health system ---
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.Initialize(unitData.maxHealth); // set current health to max from data

        // --- Link health bar UI to this unit's health system ---
        if (worldHealthBar != null)
            worldHealthBar.SetHealthSystem(healthSystem); // makes UI follow health changes
    }
}
