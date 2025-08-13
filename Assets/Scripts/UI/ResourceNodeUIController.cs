using TMPro;
using UnityEngine;

/// <summary>
/// Controls the UI panel that displays detailed information about a selected ResourceNode.
/// Handles showing, hiding, and live-updating the displayed resource amount.
/// Designed to work with any Canvas render mode (Screen Space Overlay, Screen Space Camera, or World Space).
/// </summary>
public class ResourceNodeUIController : MonoBehaviour
{
    [Header("UI Elements")]
    [SerializeField] private GameObject panel; // The main info panel to display resource node details
    [SerializeField] private TextMeshProUGUI resourceNameText; // UI text for displaying the resource type name
    [SerializeField] private TextMeshProUGUI resourceAmountText; // UI text for displaying the remaining resource amount

    private ResourceNode currentNode; // Reference to the currently selected resource node (null if none is selected)
    private Camera mainCamera; // Cached reference to the main camera for raycasting
    private RectTransform panelRectTransform; // Cached RectTransform for positioning the panel
    private Canvas parentCanvas; // Reference to the parent Canvas to determine render mode

    private void Awake()
    {
        // Cache references to improve performance and avoid repeated GetComponent calls
        mainCamera = Camera.main;
        panelRectTransform = panel.GetComponent<RectTransform>();
        parentCanvas = panel.GetComponentInParent<Canvas>();

        // Ensure the panel is hidden when the game starts
        HidePanel();
    }

    private void Update()
    {
        HandleMouseClick();

        // If a resource node is currently displayed, update its amount in real time
        if (currentNode != null)
        {
            UpdateResourceAmount(currentNode.amount);
        }
    }

    /// <summary>
    /// Checks for left mouse clicks and determines whether the player clicked on a ResourceNode.
    /// </summary>
    private void HandleMouseClick()
    {
        if (Input.GetMouseButtonDown(0)) // Left mouse button
        {
            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            // Raycast into the scene to detect clicked objects
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                ResourceNode node = hit.collider.GetComponent<ResourceNode>();

                if (node != null)
                {
                    // A resource node was clicked — display its info
                    ShowNodeInfo(node);
                }
                else
                {
                    // Clicked somewhere else — hide the info panel
                    HidePanel();
                }
            }
        }
    }

    /// <summary>
    /// Displays the UI panel with information about the given resource node.
    /// Positions the panel at the center of the screen regardless of canvas type.
    /// </summary>
    private void ShowNodeInfo(ResourceNode node)
    {
        currentNode = node;

        // Update UI texts
        resourceNameText.text = node.resourceType.ToString();
        UpdateResourceAmount(node.amount);

        // Make the panel visible
        panel.SetActive(true);

        // Positioning logic depends on the Canvas render mode
        if (parentCanvas.renderMode == RenderMode.ScreenSpaceOverlay ||
            parentCanvas.renderMode == RenderMode.ScreenSpaceCamera)
        {
            // For screen-space canvases, anchoredPosition (0,0) means panel is centered
            panelRectTransform.anchoredPosition = Vector2.zero;
        }
        else
        {
            // For world-space canvases, position the panel directly in front of the camera
            panelRectTransform.position = mainCamera.ViewportToWorldPoint(
                new Vector3(0.5f, 0.5f, mainCamera.nearClipPlane + 1f)
            );
        }
    }

    /// <summary>
    /// Updates only the displayed amount text for the resource node.
    /// Called each frame while a node is actively selected.
    /// </summary>
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
