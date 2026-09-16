using UnityEngine;
using UnityEngine.UI;

public class Health : MonoBehaviour
{
    [Header("Health Settings")]
    public int lives = 3;

    [Header("Life Images")]
    public Image[] lifeImages = new Image[3];

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            LoseLife();
        }
    }

    void LoseLife()
    {
        if (lives <= 0)
            return;

        lives--;

        // Disable the heart image that was lost
        if (lives < lifeImages.Length && lifeImages[lives] != null)
        {
            lifeImages[lives].enabled = false;
        }

        Debug.Log("Lives remaining: " + lives);

        if (lives <= 0)
        {
            Debug.Log("Player has no lives left!");
        }
    }
}