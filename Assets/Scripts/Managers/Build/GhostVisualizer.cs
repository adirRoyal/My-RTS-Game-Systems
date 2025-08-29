using UnityEngine;

/// <summary>
/// Controls the ghost building visual feedback for placement.
/// Changes the material to indicate valid or invalid placement.
/// </summary>
public class GhostVisualizer : MonoBehaviour
{
    [SerializeField] private Material validMaterial;   // Material used when placement is valid
    [SerializeField] private Material invalidMaterial; // Material used when placement is invalid

    private Renderer[] renderers; // Cached renderers of the ghost object

    private void Awake()
    {
        // --- Cache all Renderer components in this object and its children ---
        renderers = GetComponentsInChildren<Renderer>();

        if (renderers.Length == 0)
        {
            Debug.LogWarning("GhostVisualizer: No Renderers found in children!");
        }
    }

    /// <summary>
    /// Sets the ghost to valid placement material.
    /// </summary>
    public void SetValid()
    {
        if (validMaterial == null) return;

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.material = validMaterial;
        }
    }

    /// <summary>
    /// Sets the ghost to invalid placement material.
    /// </summary>
    public void SetInvalid()
    {
        if (invalidMaterial == null) return;

        foreach (Renderer r in renderers)
        {
            if (r != null)
                r.material = invalidMaterial;
        }
    }
}

    // --- Optional Improvements ---
    // 1. Consider using MaterialPropertyBlock to avoid instantiating multiple material instances
