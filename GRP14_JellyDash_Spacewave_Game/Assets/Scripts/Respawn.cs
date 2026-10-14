
using UnityEngine;

public class Respawn : MonoBehaviour
{
    [SerializeField] private Transform[] checkpoints;

    private Transform currentCheckpoint;

    private void Start()
    {
        if (checkpoints != null && checkpoints.Length > 0)
            currentCheckpoint = checkpoints[0];
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

    public void SetCheckpoint(Transform newCheckpoint)
    {
        if (checkpoints == null || checkpoints.Length == 0)
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
