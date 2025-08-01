using UnityEngine;

public class GroundMarkerController : MonoBehaviour
{
    [Header("Marker Settings")]
    [SerializeField] private GameObject groundMarkerPrefab;
    [SerializeField] private float markerDuration = 1.5f;

    [Header("Dependencies")]
    [SerializeField] private UnitSelectionHandler selectionHandler;

    private GameObject currentMarker;

    private void OnEnable()
    {
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick += ShowMarker;
        }
    }

    private void OnDisable()
    {
        if (selectionHandler != null)
        {
            selectionHandler.OnGroundClick -= ShowMarker;
        }
    }

    private void ShowMarker(Vector3 position)
    {
        if (currentMarker != null)
        {
            Destroy(currentMarker);
        }

        // מיקום טיפה מעל הקרקע + סיבוב של 90° בציר X
        Vector3 markerPosition = position + Vector3.up * 0.01f;
        Quaternion markerRotation = Quaternion.Euler(90f, 0f, 0f);

        currentMarker = Instantiate(groundMarkerPrefab, markerPosition, markerRotation);

        Destroy(currentMarker, markerDuration);
    }
}
