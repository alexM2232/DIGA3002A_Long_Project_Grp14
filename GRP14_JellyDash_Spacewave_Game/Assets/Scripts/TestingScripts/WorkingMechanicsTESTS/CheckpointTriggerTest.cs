using UnityEngine;

public class CheckpointTriggerTest : MonoBehaviour
{
    [Header("Win Checkpoint")]
    [SerializeField] private bool isWinCheckpoint = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        RespawnTest respawn = other.GetComponent<RespawnTest>();

        if (respawn != null)
        {
            // Update the player's checkpoint
            respawn.SetCheckpoint(transform);

            Debug.Log("Checkpoint reached: " + gameObject.name);

            // If this is the final checkpoint, trigger the win
            if (isWinCheckpoint)
            {
                GameUIManagerTest gameUI =
                    FindFirstObjectByType<GameUIManagerTest>();

                if (gameUI != null)
                {
                    gameUI.ShowWinPanel();

                    Debug.Log("WIN CHECKPOINT REACHED - Win Panel shown.");
                }
                else
                {
                    Debug.LogError(
                        "CheckpointTriggerTest: GameUIManagerTest could not be found in the scene."
                    );
                }
            }
        }
    }
}