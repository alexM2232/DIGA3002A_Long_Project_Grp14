using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int lives = 3;

    [Header("Life Images (leave empty to auto-find by tag 'Heart')")]
    public Image[] lifeImages;

    private void Awake()
    {
        // If nothing is assigned in the Inspector, auto-find all hearts by tag
        if (lifeImages == null || lifeImages.Length == 0)
        {
            AutoFindHearts();
        }
    }

    private void Start()
    {
        UpdateLifeUI();
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            LoseLife();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
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
        Debug.Log("Lives remaining: " + lives);

        UpdateLifeUI();

        if (lives <= 0)
        {
            Debug.Log("Player has no lives left!");
            // Add game over logic here
        }
    }

    void UpdateLifeUI()
    {
        if (lifeImages == null || lifeImages.Length == 0)
        {
            Debug.LogWarning("Health: No life images found. Tag your hearts with 'Heart'.");
            return;
        }

        for (int i = 0; i < lifeImages.Length; i++)
        {
            if (lifeImages[i] == null) continue;
            lifeImages[i].enabled = i < lives;
        }
    }

    void AutoFindHearts()
    {
        // Find every active GameObject tagged "Heart"
        GameObject[] heartObjects = GameObject.FindGameObjectsWithTag("Heart");

        if (heartObjects.Length == 0)
        {
            Debug.LogWarning("Health: No GameObjects tagged 'Heart' were found in the scene.");
            return;
        }

        // Sort them left-to-right by their X position so order is correct
        System.Array.Sort(heartObjects, (a, b) =>
            a.transform.position.x.CompareTo(b.transform.position.x));

        lifeImages = new Image[heartObjects.Length];
        for (int i = 0; i < heartObjects.Length; i++)
        {
            lifeImages[i] = heartObjects[i].GetComponent<Image>();
        }

        Debug.Log($"Health: Auto-connected {lifeImages.Length} heart images.");
    }
}