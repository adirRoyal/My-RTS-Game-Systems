using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Generic Pool Manager for frequently instantiated/destroyed prefabs.
/// Helps improve performance by reusing GameObjects instead of creating/destroying them all the time.
/// </summary>
public class PoolManager : MonoBehaviour
{
    // Singleton instance for global access
    public static PoolManager Instance { get; private set; }

    // Dictionary to store queues of pooled objects for each prefab
    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new();

    private void Awake()
    {
        // Ensure only one instance exists
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// Get an object from the pool (or instantiate a new one if none available)
    /// </summary>
    /// <param name="prefab">The prefab to get from the pool</param>
    /// <returns>Active GameObject ready to use</returns>
    public GameObject GetFromPool(GameObject prefab)
    {
        // If no queue exists for this prefab, create one
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        // If there is an object in the pool, reuse it
        if (poolDictionary[prefab].Count > 0)
        {
            GameObject obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true); // make it visible and active
            return obj;
        }

        // No object available, instantiate a new one
        return Instantiate(prefab);
    }

    /// <summary>
    /// Return an object to the pool instead of destroying it
    /// </summary>
    /// <param name="prefab">The prefab type the object belongs to</param>
    /// <param name="obj">The GameObject to return to the pool</param>
    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false); // hide the object
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }
        poolDictionary[prefab].Enqueue(obj); // add it back to the pool
    }
}
