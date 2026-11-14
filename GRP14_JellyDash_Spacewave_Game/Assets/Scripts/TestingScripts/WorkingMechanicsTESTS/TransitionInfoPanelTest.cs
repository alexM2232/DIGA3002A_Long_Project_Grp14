using UnityEngine;

public class TransitionInfoPanelTest : MonoBehaviour
{
    [Header("Information Panel")]
    [SerializeField] private GameObject informationPanel;

    [Header("Display Settings")]
    [SerializeField] private float displayTime = 2f;

    private bool hasShown = false;

    private void Start()
    {
        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player"))
            return;

        if (hasShown)
            return;

        hasShown = true;

        if (informationPanel != null)
        {
            informationPanel.SetActive(true);

            Invoke(
                nameof(HidePanel),
                displayTime
            );
        }
    }

    private void HidePanel()
    {
        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }
    }
}