using UnityEngine;

public class HitVisualEffect : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;

    public void PlayEffect(Vector3 position)
    {
        if (hitEffectPrefab == null) return;

        GameObject effect = PoolManager.Instance.GetFromPool(hitEffectPrefab);
        effect.transform.position = position;
        effect.transform.rotation = Quaternion.identity;

        StartCoroutine(ReturnAfterSeconds(effect, hitEffectPrefab, 1f));
    }

    private System.Collections.IEnumerator ReturnAfterSeconds(GameObject obj, GameObject prefab, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        PoolManager.Instance.ReturnToPool(prefab, obj);
    }
}
