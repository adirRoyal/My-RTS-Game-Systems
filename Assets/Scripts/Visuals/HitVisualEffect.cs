using UnityEngine;

public class HitVisualEffect : MonoBehaviour
{
    [SerializeField] private GameObject hitEffectPrefab; // Prefab for hit effect (e.g. particles)

    public void PlayEffect(Vector3 position)
    {
        if (hitEffectPrefab != null)
        {
            // Create hit effect at given position with no rotation
            GameObject effect = Instantiate(hitEffectPrefab, position, Quaternion.identity);

            // Destroy the effect object after 1 second to clean up
            Destroy(effect, 1f);
        }
    }
}
