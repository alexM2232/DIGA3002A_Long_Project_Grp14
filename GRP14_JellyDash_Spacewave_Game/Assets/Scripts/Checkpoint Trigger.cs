using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color inactiveColor = new Color(1f, 0.5f, 0f);
    [SerializeField] private Color activeColor = Color.green;

    [Header("Win Checkpoint")]
    [SerializeField] private bool isWinCheckpoint = false;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    private void Awake()
    {
        // Finds the SpriteRenderer on this object or its children
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning(
                $"[CheckpointTrigger] No SpriteRenderer found on '{gameObject.name}' or its children."
            );
        }
        else
        {
            spriteRenderer.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
        {
            Debug.Log(
                $"[CheckpointTrigger] Triggered by '{other.name}'"
            );
        }

        Respawn respawn = other.GetComponent<Respawn>();

        if (respawn == null)
        {
            if (showDebugLogs)
            {
                Debug.Log(
                    $"[CheckpointTrigger] '{other.name}' has no Respawn component - ignoring."
                );
            }

            return;
        }

        // Set this checkpoint as the player's respawn point
        respawn.SetCheckpoint(transform);

        // Change checkpoint colour to green
        if (!isActive)
        {
            isActive = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = activeColor;
            }

            if (showDebugLogs)
            {
                Debug.Log(
                    $"[CheckpointTrigger] '{gameObject.name}' activated."
                );
            }
        }

        // Check if this is the final checkpoint
        if (isWinCheckpoint)
        {
            GameUIManagerTest gameUI =
                FindFirstObjectByType<GameUIManagerTest>();

            if (gameUI != null)
            {
                gameUI.ShowWinPanel();

                if (showDebugLogs)
                {
                    Debug.Log(
                        "[CheckpointTrigger] Final checkpoint reached. Win Panel triggered."
                    );
                }
            }
            else
            {
                Debug.LogError(
                    "[CheckpointTrigger] GameUIManagerTest could not be found in the scene."
                );
            }
        }
    }
}