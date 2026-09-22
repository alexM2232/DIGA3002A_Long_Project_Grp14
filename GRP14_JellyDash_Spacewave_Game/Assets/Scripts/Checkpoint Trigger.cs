using UnityEngine;

public class CheckpointTrigger : MonoBehaviour
{
    [Header("Colors")]
    [SerializeField] private Color inactiveColor = new Color(1f, 0.5f, 0f); // Orange
    [SerializeField] private Color activeColor = Color.green;

    [Header("Debug")]
    [SerializeField] private bool showDebugLogs = false;

    private SpriteRenderer spriteRenderer;
    private bool isActive = false;

    private void Awake()
    {
        // Works whether the SpriteRenderer is on this object OR a child
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (spriteRenderer == null)
        {
            Debug.LogWarning($"[CheckpointTrigger] No SpriteRenderer found on '{gameObject.name}' or its children.");
        }
        else
        {
            spriteRenderer.color = inactiveColor;
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (showDebugLogs)
            Debug.Log($"[CheckpointTrigger] Triggered by '{other.name}'");

        Respawn respawn = other.GetComponent<Respawn>();
        if (respawn == null)
        {
            if (showDebugLogs)
                Debug.Log($"[CheckpointTrigger] '{other.name}' has no Respawn component — ignoring.");
            return;
        }

        respawn.SetCheckpoint(transform);

        if (!isActive)
        {
            isActive = true;

            if (spriteRenderer != null)
            {
                spriteRenderer.color = activeColor;

                if (showDebugLogs)
                    Debug.Log($"[CheckpointTrigger] '{gameObject.name}' activated — color set to green.");
            }
        }
    }
}