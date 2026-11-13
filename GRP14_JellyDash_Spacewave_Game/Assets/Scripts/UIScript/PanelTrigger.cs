
using UnityEngine;

public class PanelTrigger : MonoBehaviour
{
    [Header("Game UI Manager")]
    [SerializeField] private GameUIManagerTest gameUIManager;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private bool hasTriggered = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered)
            return;

        if (!other.CompareTag(playerTag))
            return;

        if (gameUIManager == null)
        {
            Debug.LogError("PanelTrigger: GameUIManagerTest is not assigned!");
            return;
        }

        hasTriggered = true;

        Debug.Log("Player reached the finish line!");

        gameUIManager.RacerFinished(true);
    }
}
