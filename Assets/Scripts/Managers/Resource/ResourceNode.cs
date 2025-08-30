using System;
using UnityEngine;

public class ResourceNode : MonoBehaviour
{
    public ResourceType resourceType; // Type of resource (Gold, Wood, etc)
    public int amount = 100;           // How much resource is left

    public event Action<int> OnAmountChanged; // Event called whenever amount changes
    public event Action OnDepleted;          // Event called when resource is finished

    private GameObject prefabReference;      // Reference to prefab for pooling

    // Initialize the resource node with type, starting amount, and prefab
    public void Initialize(ResourceType type, int initialAmount, GameObject prefab)
    {
        resourceType = type;
        amount = initialAmount;
        prefabReference = prefab;
        gameObject.SetActive(true); // Make sure object is visible
    }

    // Gather a certain amount of resource
    public int Gather(int gatherAmount)
    {
        // Take either requested amount or whatever is left
        int taken = Mathf.Min(gatherAmount, amount);
        amount -= taken;

        // Notify anyone listening that amount changed
        OnAmountChanged?.Invoke(amount);

        // If resource is depleted
        if (amount <= 0)
        {
            OnDepleted?.Invoke(); // Notify listeners

            if (prefabReference != null)
                PoolManager.Instance.ReturnToPool(prefabReference, gameObject); // send back to pool
            else
                Destroy(gameObject); // no pool ? just destroy
        }

        return taken; // return how much was actually taken
    }
}
