using UnityEngine;
using System.Collections;

public class GroundMarkerController : MonoBehaviour
{
    [SerializeField] private GameObject groundMarkerPrefab;
    [SerializeField] private float markerDuration = 1.5f;
    [SerializeField] private UnitSelectionHandler selectionHandler;

    private GameObject currentMarker;           // ������ ������
    private Coroutine returnCoroutine;          // �������� ������� �� ������

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
        // �� �� ����� ����, ���� ���� ��� ������ ���� ��������
        if (currentMarker != null)
        {
            if (returnCoroutine != null)
                StopCoroutine(returnCoroutine);

            PoolManager.Instance.ReturnToPool(groundMarkerPrefab, currentMarker);
            currentMarker = null;
        }

        // ��� ����� ��� �������
        currentMarker = PoolManager.Instance.GetFromPool(groundMarkerPrefab);
        currentMarker.transform.position = position + Vector3.up * 0.01f;
        currentMarker.transform.rotation = Quaternion.Euler(90f, 0f, 0f);

        // ���� �� �������� ������ ������ ���� ���
        returnCoroutine = StartCoroutine(ReturnAfterSeconds(currentMarker, groundMarkerPrefab, markerDuration));
    }

    private IEnumerator ReturnAfterSeconds(GameObject obj, GameObject prefab, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PoolManager.Instance.ReturnToPool(prefab, obj);
        currentMarker = null;    // ��� �� ������ ������
        returnCoroutine = null;  // ��� �� ��������
    }
}
