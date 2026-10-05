using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManagerTest : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

    private bool gameEnded = false;

    private void Awake()
    {
        // Hide panels when the game starts
        if (deathPanel != null)
        {
            deathPanel.SetActive(false);
        }

        if (winPanel != null)
        {
            winPanel.SetActive(false);
        }

        // Make sure the game starts normally
        Time.timeScale = 1f;

        Cursor.visible = false;
        Cursor.lockState = CursorLockMode.Locked;
    }

    public void ShowDeathPanel()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Death Panel shown.");
    }

    public void ShowWinPanel()
    {
        if (gameEnded)
        {
            return;
        }

        gameEnded = true;

        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;

        Debug.Log("Win Panel shown.");
    }

    public void Retry()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene(
            SceneManager.GetActiveScene().name
        );
    }

    public void BackToMainMenu()
    {
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}