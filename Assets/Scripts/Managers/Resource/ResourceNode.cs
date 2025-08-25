using System;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType;
    public int amount = 100;

    public event Action<int> OnAmountChanged;
    public event Action OnDepleted;

    private GameObject prefabReference; // Prefab המקורי של ה-ResourceNode

    public void Initialize(ResourceType type, int initialAmount, GameObject prefab)
    {
        resourceType = type;
        amount = initialAmount;
        prefabReference = prefab;
        gameObject.SetActive(true);
    }

    public int Gather(int gatherAmount)
    {
        int taken = Mathf.Min(gatherAmount, amount);
        amount -= taken;
        OnAmountChanged?.Invoke(amount);

        if (amount <= 0)
        {
            OnDepleted?.Invoke();
            PoolManager.Instance.ReturnToPool(prefabReference, gameObject);
        }
        return taken;
    }
}
