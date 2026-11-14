using UnityEngine;
using System.Collections;

public class TriggerDisabler : MonoBehaviour
{
    [Tooltip("Tag used to identify the player")]
    [SerializeField] private string playerTag = "Player";

    [Tooltip("Delay in seconds before disabling the trigger")]
    [SerializeField] private float disableDelay = 2f;

    [Tooltip("The collider whose isTrigger will be disabled. Defaults to the collider on this GameObject.")]
    [SerializeField] private Collider targetCollider;

    private bool hasTriggered = false;

    private void Awake()
    {
        // If no target collider was assigned, try to get one from this GameObject
        if (targetCollider == null)
        {
            targetCollider = GetComponent<Collider>();
        }

        if (targetCollider == null)
        {
            Debug.LogError($"[TriggerDisabler] No Collider found on {gameObject.name}. Please assign one.");
        }
    }

    private void OnTriggerEnter(Collider other)
    {
        // Only respond to the player and only once
        if (hasTriggered) return;
        if (!other.CompareTag(playerTag)) return;

        hasTriggered = true;
        StartCoroutine(DisableTriggerAfterDelay());
    }

    private IEnumerator DisableTriggerAfterDelay()
    {
        // Wait for the specified delay
        yield return new WaitForSeconds(disableDelay);

        if (targetCollider != null)
        {
            targetCollider.isTrigger = false;
            Debug.Log($"[TriggerDisabler] isTrigger disabled on {targetCollider.name} after {disableDelay}s.");
        }
    }
}
