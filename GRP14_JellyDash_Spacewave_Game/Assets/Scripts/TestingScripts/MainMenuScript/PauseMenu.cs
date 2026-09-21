using UnityEngine;
using UnityEngine.SceneManagement;

public class PauseMenu : MonoBehaviour
{
    [Header("Pause Menu")]
    [SerializeField] private GameObject pausePanel;

    private bool isPaused = false;

    private void Start()
    {
        // Make sure the pause menu is hidden when the game starts
        pausePanel.SetActive(false);

        // Make sure the game starts unpaused
        Time.timeScale = 1f;

        // Lock the cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    private void Update()
    {
        // Press ESC to open/close pause menu
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isPaused)
            {
                ResumeGame();
            }
            else
            {
                PauseGame();
            }
        }
    }

    public void PauseGame()
    {
        isPaused = true;

        pausePanel.SetActive(true);

        // Stop the game
        Time.timeScale = 0f;

        // Show cursor for menu buttons
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void ResumeGame()
    {
        isPaused = false;

        pausePanel.SetActive(false);

        // Continue the game
        Time.timeScale = 1f;

        // Hide cursor for gameplay
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void ReturnToMainMenu()
    {
        // Make sure time is running before changing scenes
        Time.timeScale = 1f;

        SceneManager.LoadScene("MainMenu");
    }
}