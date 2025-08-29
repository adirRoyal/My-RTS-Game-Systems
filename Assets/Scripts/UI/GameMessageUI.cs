using UnityEngine;
using TMPro;
using System.Collections;

/// <summary>
/// Handles displaying short messages to the player, like resource errors or notifications.
/// </summary>
public class GameMessageUI : MonoBehaviour
{
    [Header("UI References")]
    [SerializeField] private TextMeshProUGUI messageText; // Text element to show messages
    [SerializeField] private float displayTime = 1f;      // Duration to show the message

    private Coroutine currentMessageRoutine;

    private void Awake()
    {
        // Ensure the message text is hidden initially
        if (messageText != null)
            messageText.gameObject.SetActive(false);
    }

    /// <summary>
    /// Shows a message to the player. Cancels any previous message currently displayed.
    /// </summary>
    public void ShowMessage(string text)
    {
        if (messageText == null) return;

        if (currentMessageRoutine != null)
            StopCoroutine(currentMessageRoutine);

        currentMessageRoutine = StartCoroutine(ShowMessageRoutine(text));
    }

    /// <summary>
    /// Coroutine to display the message for a set duration and then hide it.
    /// </summary>
    private IEnumerator ShowMessageRoutine(string text)
    {
        messageText.text = text;
        messageText.gameObject.SetActive(true);

        yield return new WaitForSeconds(displayTime);

        messageText.gameObject.SetActive(false);
    }

    // --- Optional Improvements ---
    // 1. Add fade-in/fade-out animations for smoother UI feedback.
    // 2. Queue multiple messages instead of replacing the previous one instantly.
    // 3. Support different message types (error, info, success) with colors/icons.
}
