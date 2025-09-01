using UnityEngine;

[RequireComponent(typeof(HealthSystem))]
public class Unit : MonoBehaviour
{
    [SerializeField] private UnitData unitData;
    [SerializeField] private HealthBar worldHealthBar; // canvas health bar מעל הראש

    private HealthSystem healthSystem;
    public UnitData Data => unitData;
    public HealthSystem Health => healthSystem;

    private void Awake()
    {
        healthSystem = GetComponent<HealthSystem>();
        healthSystem.Initialize(unitData.maxHealth);

        if (worldHealthBar != null)
            worldHealthBar.SetHealthSystem(healthSystem); // קישור לבריאות
    }
}
