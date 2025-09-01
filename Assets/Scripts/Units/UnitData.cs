using UnityEngine;

/// <summary>
/// ScriptableObject holding data for a Unit.
/// This allows designers to create different units without touching code.
/// </summary>
[CreateAssetMenu(menuName = "RTS/Unit Data")] // Allows creating via Assets > Create > RTS > Unit Data
public class UnitData : ScriptableObject
{
    [Header("Basic Info")]
    public string unitName;   // Display name of the unit
    public Sprite unitIcon;   // Icon shown in UI (selection panel, unit list, etc.)
    public int maxHealth;     // Maximum health of the unit

    // TODO: In the future, add more properties like:
    // public int attackPower;
    // public float movementSpeed;
    // public int cost;
}
