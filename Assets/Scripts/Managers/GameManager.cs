using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }  // Singleton instance for global access

    public ResourceManager ResourceManager => resourceManager; // Expose ResourceManager via property

    private ResourceManager resourceManager;                   // Core resource management logic

    [SerializeField] private ResourceUIController resourceUIController; // Reference to UI controller

    private void Awake()
    {
        // Ensure only one instance of GameManager exists (singleton pattern)
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        resourceManager = new ResourceManager(); // Initialize resource manager
    }

    private void Start()
    {
        // Initialize starting resources
        resourceManager.AddResource(ResourceType.Wood, 100);
        resourceManager.AddResource(ResourceType.Gold, 50);

        // Initialize UI with current resource manager
        resourceUIController.Initialize(resourceManager);

        // Subscribe to resource change event to log updates
        resourceManager.OnResourceAmountChanged += (type, amount) =>
        {
            Debug.Log($"Resource {type} changed. New amount: {amount}");
        };
    }

    private void Update()
    {
        // For testing: Press Space to try consuming resources
        if (Input.GetKeyDown(KeyCode.Space))
        {
            var cost = new Dictionary<ResourceType, int>()
            {
                { ResourceType.Wood, 30 },
                { ResourceType.Gold, 20 }
            };

            if (resourceManager.ConsumeResources(cost))
            {
                Debug.Log("Successfully paid resources.");
            }
            else
            {
                Debug.Log("Not enough resources!");
            }
        }
    }
}
