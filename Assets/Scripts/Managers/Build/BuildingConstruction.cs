using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

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

    /// <summary>
    /// Initializes the construction with building data and target position.
    /// </summary>
    public void Initialize(BuildingData buildingData, Vector3 position)
    {
        data = buildingData;
        finalPosition = position;
        buildTime = data.buildTime;

        // --- Instantiate the preview building (ghost) ---
        previewInstance = Instantiate(data.prefab, position + Vector3.down * 5f, Quaternion.identity, transform);

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
        progressBar.transition = Selectable.Transition.None;
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

            // --- Animate preview rising from underground ---
            previewInstance.transform.position = Vector3.Lerp(startPos, targetPos, t);

            // --- Update progress bar and text ---
            if (progressBar != null) progressBar.value = elapsed;
            if (progressText != null)
            {
                float timeLeft = Mathf.Ceil(buildTime - elapsed);
                progressText.text = $"{timeLeft:0}s";
            }

            yield return null;
        }

        // --- Finalize construction: instantiate the real building ---
        Instantiate(data.prefab, finalPosition, Quaternion.identity);

        // --- Add supply if building provides it ---
        if (data.providesPopulation)
        {
            GameManager.Instance.ResourceManager.AddSupplyCap(data.populationProvided);
        }

        // --- Clean up preview and UI ---
        Destroy(gameObject);
    }

    // --- Optional Improvements ---
    // 1. Add construction sound or particle effects during BuildRoutine.
    // 2. Smooth progress bar color gradient from red to green as construction progresses.
    // 3. Use object pooling for prefab instances to reduce runtime instantiation overhead.
}
