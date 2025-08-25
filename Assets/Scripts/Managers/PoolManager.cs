using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Pool manager גנרי לכל Prefab שנרצה Instantiate/Destroy בתדירות גבוהה
/// </summary>
public class PoolManager : MonoBehaviour
{
    public static PoolManager Instance { get; private set; }

    private Dictionary<GameObject, Queue<GameObject>> poolDictionary = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
    }

    /// <summary>
    /// מקבל Prefab ומחזיר אובייקט מוכן לשימוש
    /// </summary>
    public GameObject GetFromPool(GameObject prefab)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }

        if (poolDictionary[prefab].Count > 0)
        {
            GameObject obj = poolDictionary[prefab].Dequeue();
            obj.SetActive(true);
            return obj;
        }

        // אם אין במאגר, צור חדש
        return Instantiate(prefab);
    }

    /// <summary>
    /// מחזיר אובייקט למאגר במקום להרוס אותו
    /// </summary>
    public void ReturnToPool(GameObject prefab, GameObject obj)
    {
        obj.SetActive(false);
        if (!poolDictionary.ContainsKey(prefab))
        {
            poolDictionary[prefab] = new Queue<GameObject>();
        }
        poolDictionary[prefab].Enqueue(obj);
    }
}
