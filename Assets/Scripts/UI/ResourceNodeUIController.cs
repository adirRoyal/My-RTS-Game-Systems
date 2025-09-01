using TMPro;
using UnityEngine;

/// <summary>
/// ResourceNodeUIController handles displaying a detailed UI panel for a selected ResourceNode.
/// This version subscribes to the centralized InputManager events instead of polling Input directly.
/// Provides live updates of resource amounts and supports different Canvas render modes.
/// </summary>
public class ResourceNodeUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel;                  // Main UI panel to display resource info
    [SerializeField] private TextMeshProUGUI resourceNameText;  // Text field to show resource type name
    [SerializeField] private TextMeshProUGUI resourceAmountText;// Text field to show remaining resource amount

    // --- Internal state ---
    private ResourceNode currentNode;          // Currently selected resource node
    private Camera mainCamera;                 // Cached reference to main camera
    private RectTransform panelRectTransform;  // Cached RectTransform of panel for positioning
    private Canvas parentCanvas;               // Parent canvas to detect render mode

    // --- Unity lifecycle ---
    private void Awake()
    {
        // Cache references for performance
        mainCamera = Camera.main;
        panelRectTransform = panel.GetComponent<RectTransform>();
        parentCanvas = panel.GetComponentInParent<Canvas>();

        // Ensure the UI panel starts hidden
        HidePanel();
    }

    private void OnEnable()
    {
        // Subscribe to left click events from InputManager
        // This allows decoupling input logic from the UI system
        InputManager.OnLeftClick += HandleLeftClick;
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks or null references
        InputManager.OnLeftClick -= HandleLeftClick;
    }

    /// <summary>
    /// Called when the player performs a left click.
    /// The InputManager provides the screen position of the click.
    /// </summary>
    /// <param name="screenPosition">Screen space coordinates of the mouse click</param>
    private void HandleLeftClick(Vector2 screenPosition)
    {
        // Convert screen position to a ray into the 3D world
        Ray ray = mainCamera.ScreenPointToRay(screenPosition);

        // Perform a raycast to detect objects under the cursor
        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            // Attempt to get a ResourceNode component from the hit object
            ResourceNode node = hit.collider.GetComponent<ResourceNode>();

            if (node != null)
            {
                // If a resource node was clicked, show its information
                ShowNodeInfo(node);
            }
            else
            {
                // Clicked somewhere else, hide the panel
                HidePanel();
            }
        }
    }

    /// <summary>
    /// Displays the UI panel and populates it with the resource node's data.
    /// </summary>
    /// <param name="node">ResourceNode to display</param>
    private void ShowNodeInfo(ResourceNode node)
    {
        currentNode = node; // Keep track of the selected node

        // Update the text fields with current resource info
        resourceNameText.text = node.resourceType.ToString();
        UpdateResourceAmount(node.amount);

        // Make the panel visible
        panel.SetActive(true);

        // Position the panel based on the canvas render mode
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ||
            parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // For screen-space canvases, center the panel using anchoredPosition
            panelRectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            // For world-space canvases, place the panel in front of the camera
            panelRectTransform.position = mainCamera.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane + 1f)
            );
        }
    }

    private void Update()
    {
        // Continuously update the displayed resource amount for the currently selected node
        // Optional improvement: subscribe to an event on the node to update only when changed
        if (currentNode != null)
        {
            UpdateResourceAmount(currentNode.amount);
        }
    }

    /// <summary>
    /// Updates only the amount text of the selected resource node.
    /// </summary>
    /// <param name="amount">Current remaining amount of the resource</param>
    private void UpdateResourceAmount(int amount)
    {
        resourceAmountText.text = "Amount: " + amount;
    }

    /// <summary>
    /// Hides the resource info panel and clears the current node reference.
    /// </summary>
    private void HidePanel()
    {
        panel.SetActive(false);
        currentNode = null;
    }
}
