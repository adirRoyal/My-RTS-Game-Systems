using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.AI;

/// <summary>
/// Handles the construction process of a building:
/// - Shows preview (ghost) building
/// - Animates building rising
/// - Shows world-space progress bar
/// - Finalizes construction
/// </summary>
public class BuildingConstruction : MonoBehaviour
{
    private BuildingData data;            // Data for the building
    private Vector3 finalPosition;        // Where the building will be placed
    private float buildTime;              // How long it takes to build

    private GameObject previewInstance;   // Ghost building for preview

    // --- World-space UI ---
    private Canvas worldCanvas;
    private Slider progressBar;
    private TextMeshProUGUI progressText;

    private NavMeshObstacle siteObstacle; // NavMeshObstacle reference for ConstructionSite

    /// <summary>
    /// Initialize construction
    /// </summary>
    public void Initialize(BuildingData buildingData, Vector3 position, NavMeshObstacle siteObstacle = null)
    {
        data = buildingData;
        finalPosition = position;
        buildTime = data.buildTime;
        this.siteObstacle = siteObstacle;

        // --- Create ghost preview ---
        previewInstance = Instantiate(data.prefab, position + Vector3.down * 5f, Quaternion.identity, transform);

        // Disable colliders while building to prevent pushing units
        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // Disable NavMeshObstacle on ghost if exists
        foreach (var obs in previewInstance.GetComponentsInChildren<NavMeshObstacle>())
            obs.enabled = false;

        // --- Create world-space progress UI ---
        CreateWorldUI();

        // --- Start building coroutine ---
        StartCoroutine(BuildRoutine());
    }

    /// <summary>
    /// Create world-space canvas, progress bar and text
    /// </summary>
    private void CreateWorldUI()
    {
        // Create canvas
        GameObject canvasGO = new GameObject("ConstructionUI");
        canvasGO.transform.SetParent(previewInstance.transform, false);
        canvasGO.transform.localPosition = Vector3.up * 10f;
        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 500;

        canvasGO.AddComponent<GraphicRaycaster>();
        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(3f, 0.5f);

        // Create slider
        GameObject sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        progressBar = sliderGO.AddComponent<Slider>();
        progressBar.minValue = 0f;
        progressBar.maxValue = buildTime;
        progressBar.value = 0f;
        progressBar.interactable = false;

        RectTransform sliderRect = progressBar.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // Create fill image
        GameObject fillGO = new GameObject("Fill");
        fillGO.transform.SetParent(sliderGO.transform, false);
        Image fillImage = fillGO.AddComponent<Image>();
        fillImage.color = Color.green;

        RectTransform fillRect = fillGO.GetComponent<RectTransform>();
        fillRect.anchorMin = Vector2.zero;
        fillRect.anchorMax = Vector2.one;
        fillRect.offsetMin = Vector2.zero;
        fillRect.offsetMax = Vector2.zero;

        progressBar.fillRect = fillRect;
        progressBar.targetGraphic = fillImage;

        // Create build time text
        GameObject textGO = new GameObject("BuildTimeText");
        textGO.transform.SetParent(sliderGO.transform, false);
        progressText = textGO.AddComponent<TextMeshProUGUI>();
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.black;
        progressText.enableAutoSizing = true;
        progressText.fontSize = 2;
        progressText.fontSizeMin = 1;
        progressText.fontSizeMax = 6;

        RectTransform textRect = progressText.GetComponent<RectTransform>();
        textRect.anchorMin = Vector2.zero;
        textRect.anchorMax = Vector2.one;
        textRect.offsetMin = Vector2.zero;
        textRect.offsetMax = Vector2.zero;
        textGO.transform.SetAsLastSibling(); // Ensure text is on top
    }

    /// <summary>
    /// Make canvas always face the camera
    /// </summary>
    private void LateUpdate()
    {
        if (worldCanvas != null && Camera.main != null)
        {
            worldCanvas.transform.LookAt(Camera.main.transform);
            worldCanvas.transform.Rotate(0, 180f, 0);
        }
    }

    /// <summary>
    /// Build animation coroutine
    /// </summary>
    private IEnumerator BuildRoutine()
    {
        Vector3 startPos = previewInstance.transform.position;
        Vector3 targetPos = finalPosition;
        float elapsed = 0f;

        while (elapsed < buildTime)
        {
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / buildTime);

            previewInstance.transform.position = Vector3.Lerp(startPos, targetPos, t);

            if (progressBar != null) progressBar.value = elapsed;
            if (progressText != null)
                progressText.text = $"{Mathf.Ceil(buildTime - elapsed):0}s";

            yield return null;
        }

        // --- Spawn final building ---
        GameObject building = Instantiate(data.prefab, finalPosition, Quaternion.identity);

        // Enable colliders
        foreach (var col in building.GetComponentsInChildren<Collider>())
            col.enabled = true;

        // Add NavMeshObstacle
        var finalObstacle = building.GetComponent<NavMeshObstacle>();
        if (finalObstacle == null) finalObstacle = building.AddComponent<NavMeshObstacle>();
        finalObstacle.shape = NavMeshObstacleShape.Box;
        finalObstacle.carving = true;
        finalObstacle.carveOnlyStationary = true;
        finalObstacle.carvingMoveThreshold = 0.1f;
        finalObstacle.carvingTimeToStationary = 0.1f;

        // Size & center from prefab collider
        BoxCollider prefabCollider = data.prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Vector3 scaledSize = Vector3.Scale(prefabCollider.size, data.prefab.transform.localScale);
            Vector3 scaledCenter = Vector3.Scale(prefabCollider.center, data.prefab.transform.localScale);
            const float padding = 0.5f;
            finalObstacle.size = new Vector3(scaledSize.x + padding, scaledSize.y, scaledSize.z + padding);
            finalObstacle.center = scaledCenter;
        }

        // Destroy construction site (preview + UI)
        Destroy(gameObject);
    }
}
