

using UnityEngine;

public class Respawn : MonoBehaviour
{
    [Header("Checkpoints")]
    [SerializeField] private Transform[] checkpoints;

    private Transform currentCheckpoint;
    private PlayerVisualController playerVisuals;

    private void Awake()
    {
        playerVisuals = GetComponent<PlayerVisualController>();
    }

    private void Start()
    {
        if (checkpoints != null && checkpoints.Length > 0)
        {
            currentCheckpoint = checkpoints[0];
        }
        else
        {
            Debug.LogWarning("Respawn: No checkpoints assigned!");
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Damage"))
            return;

        if (currentCheckpoint == null)
        {
            Debug.LogWarning("Respawn: No checkpoint set!");
            return;
        }

        // Move player to checkpoint.
        transform.position = currentCheckpoint.position;

        Rigidbody2D rb = GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        // Clear and restart Space Waves trail if applicable.
        if (playerVisuals != null)
            playerVisuals.OnPlayerRespawn();

        Debug.Log("Player respawned at: " + currentCheckpoint.name);
    }

    public void SetCheckpoint(Transform newCheckpoint)
    {
        if (newCheckpoint == null || checkpoints == null)
            return;

        int newIndex = System.Array.IndexOf(checkpoints, newCheckpoint);
        int currentIndex = System.Array.IndexOf(checkpoints, currentCheckpoint);

        if (newIndex > currentIndex)
        {
            currentCheckpoint = newCheckpoint;

            if (AudioManager.Instance != null)
                AudioManager.Instance.PlayCheckpointSFX();

            Debug.Log("New checkpoint reached: " + newCheckpoint.name);
        }
    }
}
