using UnityEngine;
using UnityEngine.UI;

public class HealthTest : MonoBehaviour
{
    [Header("Health")]
    [SerializeField] private int startingLives = 3;

    [Header("Heart UI")]
    [SerializeField] private Image[] lifeImages;

    private int lives;

    private RespawnTest respawn;
    private GameUIManagerTest gameUI;

    public int CurrentLives => lives;

    private void Awake()
    {
        lives = startingLives;

        respawn =
            GetComponent<RespawnTest>();

        gameUI =
            FindFirstObjectByType<GameUIManagerTest>();

        if (lifeImages == null ||
            lifeImages.Length == 0)
        {
            AutoFindHearts();
        }
    }

    private void Start()
    {
        UpdateLifeUI();
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            LoseLife();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (other.CompareTag("Damage"))
        {
            LoseLife();
        }
    }

    public void LoseLife()
    {
        if (lives <= 0)
            return;

        lives--;

        Debug.Log(
            "Lives remaining: " +
            lives
        );

        UpdateLifeUI();

        if (lives <= 0)
        {
            Debug.Log(
                "Player has no lives left."
            );

            if (gameUI != null)
            {
                gameUI.ShowDeathPanel();
            }
        }
        else
        {
            if (respawn != null)
            {
                respawn.RespawnPlayer();
            }
        }
    }

    private void UpdateLifeUI()
    {
        if (lifeImages == null ||
            lifeImages.Length == 0)
        {
            Debug.LogWarning(
                "HealthTest: No heart images assigned."
            );

            return;
        }

        for (int i = 0;
             i < lifeImages.Length;
             i++)
        {
            if (lifeImages[i] != null)
            {
                lifeImages[i].enabled =
                    i < lives;
            }
        }
    }

    private void AutoFindHearts()
    {
        GameObject[] hearts =
            GameObject.FindGameObjectsWithTag("Heart");

        if (hearts.Length == 0)
        {
            Debug.LogWarning(
                "HealthTest: No objects tagged 'Heart' found."
            );

            return;
        }

        System.Array.Sort(
            hearts,
            (a, b) =>
                a.transform.position.x.CompareTo(
                    b.transform.position.x
                )
        );

        lifeImages =
            new Image[hearts.Length];

        for (int i = 0;
             i < hearts.Length;
             i++)
        {
            lifeImages[i] =
                hearts[i].GetComponent<Image>();
        }
    }
}