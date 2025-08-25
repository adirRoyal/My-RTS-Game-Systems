using UnityEngine;
using System.Collections;

public class GroundMarkerController : MonoBehaviour
{
    [SerializeField] private GameObject groundMarkerPrefab;
    [SerializeField] private float markerDuration = 1.5f;
    [SerializeField] private UnitSelectionHandler selectionHandler;

    private GameObject currentMarker;           // הסימון הנוכחי
    private Coroutine returnCoroutine;          // הקרוטינה שהחזיקה את הסימון

    private void OnEnable()
    {
        if (selectionHandler != null)
            selectionHandler.OnGroundClick += ShowMarker;
    }

    private void OnDisable()
    {
        if (selectionHandler != null)
            selectionHandler.OnGroundClick -= ShowMarker;
    }

    private void ShowMarker(Vector3 position)
    {
        // אם יש סימון קודם, החזר אותו מיד לבריכה ובטל הקרוטינה
        if (currentMarker != null)
        {
            if (returnCoroutine != null)
                StopCoroutine(returnCoroutine);

            PoolManager.Instance.ReturnToPool(groundMarkerPrefab, currentMarker);
            currentMarker = null;
        }

        // קבל סימון חדש מהבריכה
        currentMarker = PoolManager.Instance.GetFromPool(groundMarkerPrefab);
        currentMarker.transform.position = position + Vector3.up * 0.01f;
        currentMarker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // הפעל את הקרוטינה להחזרת הסימון לאחר זמן
        returnCoroutine = StartCoroutine(ReturnAfterSeconds(currentMarker, groundMarkerPrefab, markerDuration));
    }

    private IEnumerator ReturnAfterSeconds(GameObject obj, GameObject prefab, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PoolManager.Instance.ReturnToPool(prefab, obj);
        currentMarker = null;    // נקה את הסימון הנוכחי
        returnCoroutine = null;  // נקה את הקרוטינה
    }
}
