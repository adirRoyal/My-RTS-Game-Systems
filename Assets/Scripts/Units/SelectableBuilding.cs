using UnityEngine;

/// <summary>
/// Special type of Selectable for buildings.
/// Can have unique logic for buildings, like opening a UI panel when selected.
/// </summary>
public class SelectableBuilding : Selectable
{
    /// <summary>
    /// Call this when the building is selected to open its UI panel.
    /// Example: show production options, upgrades, or building info.
    /// </summary>
    public void OpenPanel()
    {
        // Log to console for debugging
        Debug.Log("Building panel opened for " + gameObject.name);

        // TODO: Here you can trigger the building-specific UI
        // Example: UIManager.Instance.ShowBuildingPanel(this);
    }
}
