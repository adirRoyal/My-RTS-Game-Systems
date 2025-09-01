using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;
using UnityEngine.AI;

/// <summary>
/// Handles the construction process of a building: spawns preview, shows world-space progress bar,
/// animates the building rising from the ground, and finalizes construction.
/// </summary>
public class BuildingConstruction : MonoBehaviour
{
    private BuildingData data;
    private Vector3 finalPosition;
    private float buildTime;

    private GameObject previewInstance;

    // --- World-space UI ---
    private Canvas worldCanvas;
    private Slider progressBar;
    private TextMeshProUGUI progressText;

    // >>> חדש: רפרנס ל-Obstacle של ה-ConstructionSite
    private NavMeshObstacle siteObstacle;


    /// <summary>
    /// Initializes the construction with building data and target position.
    /// </summary>
    public void Initialize(BuildingData buildingData, Vector3 position, NavMeshObstacle siteObstacle = null)
    {
        data = buildingData;
        finalPosition = position;
        buildTime = data.buildTime;
        this.siteObstacle = siteObstacle;

        // --- Instantiate the preview building (ghost) ---
        previewInstance = Instantiate(data.prefab, position + Vector3.down * 5f, Quaternion.identity, transform);

        // >>> חשוב: לכבות כל הקוליידרים בזמן הבנייה כדי שלא ידחוף/יתקע Agents
        foreach (var col in previewInstance.GetComponentsInChildren<Collider>())
            col.enabled = false;

        // אם יש בטעות NavMeshObstacle על הפריפאב – נכבה גם אותו בזמן הבנייה
        foreach (var obs in previewInstance.GetComponentsInChildren<NavMeshObstacle>())
            obs.enabled = false;

        // --- Create world-space UI for progress ---
        CreateWorldUI();

        // --- Start building coroutine ---
        StartCoroutine(BuildRoutine());
    }

    /// <summary>
    /// Creates a world-space canvas with progress bar and text above the preview.
    /// </summary>
    private void CreateWorldUI()
    {
        // --- Canvas ---
        GameObject canvasGO = new GameObject("ConstructionUI");
        canvasGO.transform.SetParent(previewInstance.transform, false);
        canvasGO.transform.localPosition = Vector3.up * 10f;
        canvasGO.transform.localRotation = Quaternion.identity;
        canvasGO.transform.localScale = Vector3.one;

        worldCanvas = canvasGO.AddComponent<Canvas>();
        worldCanvas.renderMode = RenderMode.WorldSpace;
        worldCanvas.sortingOrder = 500;

        CanvasScaler scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.dynamicPixelsPerUnit = 10f;

        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = worldCanvas.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(3f, 0.5f);

        // --- Slider (ProgressBar) ---
        GameObject sliderGO = new GameObject("ProgressBar");
        sliderGO.transform.SetParent(canvasGO.transform, false);
        progressBar = sliderGO.AddComponent<Slider>();
        progressBar.interactable = false;
        progressBar.transition = UnityEngine.UI.Selectable.Transition.None;
        progressBar.minValue = 0f;
        progressBar.maxValue = buildTime;
        progressBar.value = 0f;

        RectTransform sliderRect = progressBar.GetComponent<RectTransform>();
        sliderRect.anchorMin = Vector2.zero;
        sliderRect.anchorMax = Vector2.one;
        sliderRect.offsetMin = Vector2.zero;
        sliderRect.offsetMax = Vector2.zero;

        // --- Fill Image ---
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

        // --- Build Time Text ---
        GameObject textGO = new GameObject("BuildTimeText");
        textGO.transform.SetParent(sliderGO.transform, false);
        progressText = textGO.AddComponent<TextMeshProUGUI>();
        progressText.alignment = TextAlignmentOptions.Center;
        progressText.color = Color.black;
        progressText.fontSize = 2; // Adjusted for world-space
        progressText.enableAutoSizing = true;
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
    /// Ensures the world-space canvas always faces the main camera.
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
    /// Coroutine for building the structure over time, animating the preview rising, updating progress bar, and finalizing construction.
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
            {
                float timeLeft = Mathf.Ceil(buildTime - elapsed);
                progressText.text = $"{timeLeft:0}s";
            }

            yield return null;
        }

        // --- Finalize: instantiate the real building ---
        GameObject building = Instantiate(data.prefab, finalPosition, Quaternion.identity);

        // >>> הפעלת הקוליידרים על הבניין האמיתי
        foreach (var col in building.GetComponentsInChildren<Collider>())
            col.enabled = true;

        // >>> הוספת NavMeshObstacle לבניין הסופי (עם אותם פרמטרים כמו ב-siteObstacle)
        var finalObstacle = building.GetComponent<NavMeshObstacle>();
        if (finalObstacle == null) finalObstacle = building.AddComponent<NavMeshObstacle>();

        finalObstacle.shape = NavMeshObstacleShape.Box;
        finalObstacle.carving = true;
        finalObstacle.carveOnlyStationary = true;
        finalObstacle.carvingMoveThreshold = 0.1f;
        finalObstacle.carvingTimeToStationary = 0.1f;

        // למדוד לפי הקוליידר של הפריפאב (סקיילד), עם padding קטן
        BoxCollider prefabCollider = data.prefab.GetComponent<BoxCollider>();
        if (prefabCollider != null)
        {
            Vector3 scaledSize = Vector3.Scale(prefabCollider.size, data.prefab.transform.localScale);
            Vector3 scaledCenter = Vector3.Scale(prefabCollider.center, data.prefab.transform.localScale);
            const float padding = 0.5f;
            finalObstacle.size = new Vector3(scaledSize.x + padding, scaledSize.y, scaledSize.z + padding);
            finalObstacle.center = scaledCenter;
        }

        // >>> להרוס את ה-ConstructionSite (כולל ה-siteObstacle) רק אחרי שהבנייה מוכנה
        Destroy(gameObject);
    }

    // CreateWorldUI() – כמו שהיה...
}
