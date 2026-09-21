using UnityEngine;
using UnityEngine.SceneManagement;

public class GameUIManagerTest : MonoBehaviour
{
    [Header("UI Panels")]
    [SerializeField] private GameObject deathPanel;
    [SerializeField] private GameObject winPanel;

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
    }

    public void ShowDeathPanel()
    {
        if (deathPanel != null)
        {
            deathPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        // Show cursor for the death menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void ShowWinPanel()
    {
        if (winPanel != null)
        {
            winPanel.SetActive(true);
        }

        Time.timeScale = 0f;

        // Show cursor for the win menu
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
    }

    public void Retry()
    {
        // Resume time before restarting
        Time.timeScale = 1f;

        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void BackToMainMenu()
    {
        // Resume time before changing scenes
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}