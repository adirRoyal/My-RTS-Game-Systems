using UnityEngine;

public class HitVisualEffect : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab;

    public void PlayEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);
            Destroy(effect, 1f); // השמדה לאחר זמן
        }
    }
}
