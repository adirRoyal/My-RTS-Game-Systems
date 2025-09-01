using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Serializable class that represents the UI elements for a single unit.
/// This class is not a MonoBehaviour, so it cannot be attached directly to a GameObject,
/// but because it's marked as [System.Serializable], it can be displayed and configured
/// in the Unity Inspector when used inside another MonoBehaviour.
/// </summary>
[System.Serializable]
public class SingleUnitUI
{
    // --- UI Panel Container ---
    // The root GameObject that holds all of the UI elements related to this unit.
    // Typically, this would be an element in a selection panel or a HUD.
    public GameObject panel;

    // --- Unit Icon/Image ---
    // A UI Image component used to display the unit's portrait, avatar, or icon.
    // This provides a quick visual representation of which unit is being displayed.
    public Image unitImage;

    // --- Health Fill ---
    // A UI Image component (usually with a filled type, e.g., "Filled - Horizontal")
    // that represents the current health of the unit.
    // This is commonly used as a health bar overlay on the unit icon.
    public Image healthFill;

    // --- Unit Name ---
    // A TextMeshPro text element that shows the unit's name.
    // This provides additional context to the player alongside the icon.
    public TMP_Text unitName;
}
