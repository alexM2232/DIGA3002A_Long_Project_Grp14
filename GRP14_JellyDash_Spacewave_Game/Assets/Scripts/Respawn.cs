using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Transform[] checkpoints;

    private Transform currentCheckpoint;

    private void Start()
    {
        // Default to the first checkpoint if none has been reached yet
        if (checkpoints != null && checkpoints.Length > 0)
        {
            currentCheckpoint = checkpoints[0];
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Damage"))
        {
            if (currentCheckpoint != null)
            {
                transform.position = currentCheckpoint.position;
            }
            else
            {
                Debug.LogWarning("No checkpoint set!");
            }
        }
    }

    // Call this from a trigger on each checkpoint object
    public void SetCheckpoint(Transform newCheckpoint)
    {
        // Only update if this checkpoint comes after the current one
        int newIndex = System.Array.IndexOf(checkpoints, newCheckpoint);
        int currentIndex = System.Array.IndexOf(checkpoints, currentCheckpoint);

        if (newIndex > currentIndex)
        {
            currentCheckpoint = newCheckpoint;
        }
    }
}