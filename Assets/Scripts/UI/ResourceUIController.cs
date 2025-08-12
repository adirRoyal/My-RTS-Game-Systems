using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class ResourceUIController : MonoBehaviour
{
    [System.Serializable]
    public struct ResourceUIElement
    {
        public ResourceType resourceType;           // Type of resource this UI element shows
        public Image resourceIcon;                   // Icon image representing resource (optional)
        public TextMeshProUGUI resourceText;        // Text to display amount and resource name
    }

    [SerializeField] private List<ResourceUIElement> resourceUIElements; // List of UI elements to update

    private ResourceManager resourceManager;      // Reference to the ResourceManager

    /// <summary>
    /// Initializes the UI controller by subscribing to the resource manager events.
    /// Also updates UI to current resource amounts right away.
    /// </summary>
    public void Initialize(ResourceManager manager)
    {
        // Unsubscribe from previous manager events to avoid duplicate updates
        if (resourceManager != null)
        {
            resourceManager.OnResourceAmountChanged -= UpdateResourceUI;
        }

        resourceManager = manager;
        resourceManager.OnResourceAmountChanged += UpdateResourceUI;

        // Update UI immediately for all resources we are tracking
        foreach (var element in resourceUIElements)
        {
            UpdateResourceUI(element.resourceType, resourceManager.GetAmount(element.resourceType));
        }
    }

    /// <summary>
    /// Updates the UI text of the specified resource type.
    /// </summary>
    private void UpdateResourceUI(ResourceType type, int amount)
    {
        foreach (var element in resourceUIElements)
        {
            if (element.resourceType == type)
            {
                element.resourceText.text = $"{type}: {amount}";
                break; // Once found and updated, break loop to save cycles
            }
        }
    }

    private void OnDestroy()
    {
        // Clean up event subscriptions when UI controller is destroyed
        if (resourceManager != null)
            resourceManager.OnResourceAmountChanged -= UpdateResourceUI;
    }
}
