using UnityEngine;

public class RespawnTest : MonoBehaviour
{
    [Header("Checkpoints")]
    [SerializeField] private Transform[] checkpoints;

    private Transform currentCheckpoint;

    private void Start()
    {
        if (checkpoints != null &&
            checkpoints.Length > 0)
        {
            currentCheckpoint =
                checkpoints[0];

            Debug.Log(
                "Starting checkpoint: " +
                currentCheckpoint.name
            );
        }
        else
        {
            Debug.LogWarning(
                "RespawnTest: No checkpoints assigned."
            );
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            RespawnPlayer();
        }
    }

    private void OnTriggerEnter2D(
        Collider2D other)
    {
        if (other.CompareTag("Damage"))
        {
            RespawnPlayer();
        }
    }

    private void RespawnPlayer()
    {
        if (currentCheckpoint == null)
        {
            Debug.LogWarning(
                "RespawnTest: No current checkpoint."
            );

            return;
        }

        transform.position =
            currentCheckpoint.position;

        Rigidbody2D rb =
            GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity =
                Vector2.zero;
        }

        Debug.Log(
            "Player respawned at: " +
            currentCheckpoint.name
        );
    }

    public void SetCheckpoint(
        Transform newCheckpoint)
    {
        if (newCheckpoint == null)
            return;

        if (checkpoints == null ||
            checkpoints.Length == 0)
        {
            Debug.LogWarning(
                "RespawnTest: Checkpoint list is empty."
            );

            return;
        }

        int newIndex =
            System.Array.IndexOf(
                checkpoints,
                newCheckpoint
            );

        int currentIndex =
            System.Array.IndexOf(
                checkpoints,
                currentCheckpoint
            );

        if (newIndex == -1)
        {
            Debug.LogWarning(
                "RespawnTest: Checkpoint is not in the array."
            );

            return;
        }

        if (currentCheckpoint == null ||
            newIndex > currentIndex)
        {
            currentCheckpoint =
                newCheckpoint;

            Debug.Log(
                "Checkpoint updated to: " +
                newCheckpoint.name
            );
        }
    }
}