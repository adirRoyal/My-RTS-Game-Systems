using UnityEngine;

public class GroundMarkerController : MonoBehaviour
{
    [Header("Marker Settings")]
    [SerializeField] private GameObject groundMarkerPrefab;  // Prefab for the ground marker
    [SerializeField] private float markerDuration = 1.5f;     // How long marker stays visible

    [Header("Dependencies")]
    [SerializeField] private UnitSelectionHandler selectionHandler; // Reference to unit selection handler

    private GameObject currentMarker; // Reference to the current active marker

    private void OnEnable()
    {
        // Subscribe to ground click event
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick += ShowMarker;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from ground click event
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick -= ShowMarker;
        }
    }

    private void ShowMarker(Vector3 position)
    {
        // If a marker already exists, destroy it
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }

        // Position marker slightly above ground and rotate it 90 degrees on X axis
        Vector3 markerPosition = position + Vector3.up * 0.01f;
        Quaternion markerRotation = Quaternion.Euler(90f, 0f, 0f);

        // Create new marker at the position with rotation
        currentMarker = Instantiate(groundMarkerPrefab, markerPosition, markerRotation);

        // Destroy marker after some time
        Destroy(currentMarker, markerDuration);
    }
}
