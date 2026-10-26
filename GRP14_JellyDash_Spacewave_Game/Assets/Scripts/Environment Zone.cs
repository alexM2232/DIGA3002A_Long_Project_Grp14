using UnityEngine;

/// <summary>
/// Place one of these at each boundary between environments.
/// When the player enters the trigger, the active environment switches to
/// <see cref="environmentId"/>, which activates that environment's backgrounds
/// and (optionally) hides the previous one.
/// </summary>
[RequireComponent(typeof(Collider2D))]
public class EnvironmentZone : MonoBehaviour
{
    [Tooltip("Id of the environment the player is entering. Must match that " +
             "environment's EndlessBackground.environmentId.")]
    public string environmentId = "Environment2";

    [Tooltip("Log each trigger to the console for debugging.")]
    public bool debugLog = true;

    void Reset()
    {
        // Auto-configure the collider when the script is added in the editor.
        var col = GetComponent<Collider2D>();
        if (col != null) col.isTrigger = true;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        if (debugLog)
            Debug.Log($"[EnvironmentZone] '{name}' hit by player → switching to '{environmentId}'");

        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.SetEnvironment(environmentId);
        else
            Debug.LogWarning("[EnvironmentZone] No EnvironmentManager found in scene.");
    }

    // Make the seam visible in the Scene view.
    void OnDrawGizmos()
    {
        var col = GetComponent<Collider2D>();
        if (col == null) return;
        Gizmos.color = new Color(0f, 1f, 0f, 0.25f);
        Gizmos.DrawCube(col.bounds.center, col.bounds.size);
    }
}