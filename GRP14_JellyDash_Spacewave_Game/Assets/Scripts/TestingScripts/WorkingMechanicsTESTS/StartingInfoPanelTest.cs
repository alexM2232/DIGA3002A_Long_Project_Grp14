using UnityEngine;

public class StartingInfoPanelTest : MonoBehaviour
{
    [Header("Information Panel")]
    [SerializeField] private GameObject informationPanel;

    [Header("Display Settings")]
    [SerializeField] private float displayTime = 2f;

    private void Start()
    {
        if (informationPanel == null)
            return;

        informationPanel.SetActive(true);

        Invoke(
            nameof(HidePanel),
            displayTime
        );
    }

    private void HidePanel()
    {
        if (informationPanel != null)
        {
            informationPanel.SetActive(false);
        }
    }
}
