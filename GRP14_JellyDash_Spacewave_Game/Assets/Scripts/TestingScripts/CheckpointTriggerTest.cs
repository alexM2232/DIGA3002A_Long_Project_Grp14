using UnityEngine;

public class CheckpointTriggerTest : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D other)
    {
        RespawnTest respawn =
            other.GetComponent<RespawnTest>();

        if (respawn != null)
        {
            respawn.SetCheckpoint(transform);

            Debug.Log(
                "Checkpoint reached: " +
                gameObject.name
            );
        }
    }
}