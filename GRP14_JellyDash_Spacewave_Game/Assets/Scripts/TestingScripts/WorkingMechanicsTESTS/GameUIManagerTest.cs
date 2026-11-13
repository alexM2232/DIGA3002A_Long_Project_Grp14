
using UnityEngine;
using UnityEngine.SceneManagement;
using TMPro;

public class GameUIManagerTest : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject winPanel;
    [SerializeField] private GameObject losePanel;

    [Header("Lose Panel Placement Text")]
    [SerializeField] private TMP_Text secondPlaceText;
    [SerializeField] private TMP_Text thirdPlaceText;
    [SerializeField] private TMP_Text fourthPlaceText;

    private int finishPosition = 0;
    private bool gameEnded = false;
    private bool playerHasFinished = false;

    private void Awake()
    {
        if (winPanel != null)
            winPanel.SetActive(false);

        if (losePanel != null)
            losePanel.SetActive(false);

        HidePlacementTexts();

        Time.timeScale = 1f;
        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void RacerFinished(bool isPlayer)
    {
        if (gameEnded)
            return;

        if (isPlayer && playerHasFinished)
            return;

        if (isPlayer)
            playerHasFinished = true;

        finishPosition++;

        Debug.Log(
            (isPlayer ? "Player" : "NPC") +
            " finished in position: " + finishPosition
        );

        if (isPlayer)
            ShowPlayerResult(finishPosition);
    }

    private void ShowPlayerResult(int position)
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (position == 1)
        {
            ShowWinPanel();
            return;
        }

        if (losePanel != null)
            losePanel.SetActive(true);

        HidePlacementTexts();

        if (position == 2 && secondPlaceText != null)
            secondPlaceText.gameObject.SetActive(true);
        else if (position == 3 && thirdPlaceText != null)
            thirdPlaceText.gameObject.SetActive(true);
        else if (position >= 4 && fourthPlaceText != null)
            fourthPlaceText.gameObject.SetActive(true);

        PauseAndShowCursor();

        Debug.Log("Player finished in position: " + position);
    }

    private void HidePlacementTexts()
    {
        if (secondPlaceText != null)
            secondPlaceText.gameObject.SetActive(false);

        if (thirdPlaceText != null)
            thirdPlaceText.gameObject.SetActive(false);

        if (fourthPlaceText != null)
            fourthPlaceText.gameObject.SetActive(false);
    }

    public void ShowWinPanel()
    {
        gameEnded = true;

        if (winPanel != null)
            winPanel.SetActive(true);

        PauseAndShowCursor();

        Debug.Log("Win Panel shown.");
    }

    public void ShowLosePanel()
    {
        if (gameEnded)
            return;

        gameEnded = true;

        if (losePanel != null)
            losePanel.SetActive(true);

        PauseAndShowCursor();

        Debug.Log("Lose Panel shown.");
    }

    // Compatibility method for other scripts that still call ShowDeathPanel().
    // It displays the existing Lose Panel. No Death Panel is required.
    public void ShowDeathPanel()
    {
        ShowLosePanel();
    }

    private void PauseAndShowCursor()
    {
        Time.timeScale = 0f;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Retry()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene("MainMenu");
    }
}
