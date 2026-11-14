
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialManager : MonoBehaviour
{
    [Header("Dialogue UI")]
    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;
    [SerializeField] private Button resumeButton;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 35f;

    [Header("Slide Animation")]
    [SerializeField] private float animationDuration = 0.45f;
    [SerializeField] private Vector2 visiblePosition = new Vector2(0f, 100f);
    [SerializeField] private Vector2 hiddenPosition = new Vector2(0f, -350f);

    private RectTransform panelRect;
    private CanvasGroup canvasGroup;

    private bool isShowingDialogue;
    private bool resumePressed;

    private Coroutine dialogueCoroutine;

    private void Awake()
    {
        Debug.Log("[TutorialManager] Initialising.");

        // Ensure the game starts at normal speed.
        Time.timeScale = 1f;

        if (dialoguePanel == null || dialogueText == null || resumeButton == null)
        {
            Debug.LogError("[TutorialManager] Assign DialoguePanel, DialogueText and ResumeButton in the Inspector.");
            enabled = false;
            return;
        }

        panelRect = dialoguePanel.GetComponent<RectTransform>();
        canvasGroup = dialoguePanel.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            Debug.LogError("[TutorialManager] Add CanvasGroup to DialoguePanel.");
            enabled = false;
            return;
        }

        resumeButton.onClick.RemoveListener(ResumeTutorial);
        resumeButton.onClick.AddListener(ResumeTutorial);

        dialoguePanel.SetActive(false);
        resumeButton.gameObject.SetActive(false);

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        panelRect.anchoredPosition = hiddenPosition;

        Debug.Log("[TutorialManager] Ready. Waiting for tutorial triggers.");
    }

    public bool TryShowTutorial(string message)
    {
        if (!enabled || isShowingDialogue)
            return false;

        if (string.IsNullOrWhiteSpace(message))
        {
            Debug.LogWarning("[TutorialManager] Dialogue message is empty.");
            return false;
        }

        isShowingDialogue = true;
        dialogueCoroutine = StartCoroutine(ShowDialogue(message));

        return true;
    }

    private IEnumerator ShowDialogue(string message)
    {
        resumePressed = false;

        // Pause gameplay while the tutorial is displayed.
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        dialoguePanel.SetActive(true);
        resumeButton.gameObject.SetActive(false);

        dialogueText.text = "";

        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = true;

        panelRect.anchoredPosition = hiddenPosition;

        yield return AnimatePanel(
            hiddenPosition,
            visiblePosition,
            0f,
            1f
        );

        yield return TypeMessage(message);

        resumeButton.gameObject.SetActive(true);
        canvasGroup.interactable = true;

        Debug.Log("[TutorialManager] Waiting for Resume button.");

        while (!resumePressed)
        {
            yield return null;
        }

        resumeButton.gameObject.SetActive(false);
        canvasGroup.interactable = false;

        yield return AnimatePanel(
            visiblePosition,
            hiddenPosition,
            1f,
            0f
        );

        dialoguePanel.SetActive(false);

        // Always restore normal gameplay speed.
        Time.timeScale = 1f;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        isShowingDialogue = false;
        dialogueCoroutine = null;

        Debug.Log("[TutorialManager] Dialogue finished. Gameplay resumed.");
    }

    private IEnumerator TypeMessage(string message)
    {
        dialogueText.text = "";

        float delay = 1f / Mathf.Max(1f, typingSpeed);

        foreach (char letter in message)
        {
            dialogueText.text += letter;
            yield return new WaitForSecondsRealtime(delay);
        }
    }

    private IEnumerator AnimatePanel(
        Vector2 startPosition,
        Vector2 endPosition,
        float startAlpha,
        float endAlpha)
    {
        float elapsed = 0f;

        while (elapsed < animationDuration)
        {
            elapsed += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(elapsed / animationDuration);

            float smoothProgress = progress * progress * (3f - 2f * progress);

            panelRect.anchoredPosition = Vector2.Lerp(
                startPosition,
                endPosition,
                smoothProgress
            );

            canvasGroup.alpha = Mathf.Lerp(
                startAlpha,
                endAlpha,
                smoothProgress
            );

            yield return null;
        }

        panelRect.anchoredPosition = endPosition;
        canvasGroup.alpha = endAlpha;
    }

    private void ResumeTutorial()
    {
        if (!isShowingDialogue)
            return;

        resumePressed = true;
    }

    private void OnDestroy()
    {
        if (resumeButton != null)
            resumeButton.onClick.RemoveListener(ResumeTutorial);

        if (isShowingDialogue)
        {
            Time.timeScale = 1f;
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;
        }
    }
}
