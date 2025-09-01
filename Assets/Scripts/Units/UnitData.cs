using UnityEngine;

[CreateAssetMenu(menuName = "RTS/Unit Data")]
public class UnitData : ScriptableObject
{
    public string unitName;
    public Sprite unitIcon;
    public int maxHealth;
    // בעתיד: התקפות, מהירות, עלות וכו'
}
