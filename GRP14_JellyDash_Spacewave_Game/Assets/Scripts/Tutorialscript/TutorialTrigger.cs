
using UnityEngine;

public class TutorialTrigger : MonoBehaviour
{
    [Header("Tutorial Manager")]
    [SerializeField] private TutorialManager tutorialManager;

    [Header("Trigger Identification")]
    [SerializeField] private string triggerID = "Introduction";

    [Header("Dialogue")]
    [TextArea(3, 6)]
    [SerializeField] private string dialogueMessage;

    [Header("Player Detection")]
    [SerializeField] private string playerTag = "Player";

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!TutorialProgress.CanShow(triggerID))
             return;
        
        bool isPlayer =
            other.CompareTag(playerTag) ||
            other.GetComponentInParent<PlayerMovementJellyDash>() != null;

        if (!isPlayer)
            return;

        if (tutorialManager == null)
        {
            Debug.LogError("TutorialManager is missing on " + gameObject.name);
            return;
        }

        bool started = tutorialManager.TryShowTutorial(dialogueMessage);

        if (!started)
            return;

        TutorialProgress.MarkAsSeen(triggerID);

        Debug.Log("Tutorial shown for the first time: " + triggerID);
    }
}
