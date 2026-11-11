using System.Collections.Generic;
using UnityEngine;

public class TutorialProgress : MonoBehaviour
{
    private static TutorialProgress instance;

    private static HashSet<string> completedTutorials =
        new HashSet<string>();

    private static readonly string[] tutorialOrder =
    {
        "Introduction",
        "Movement",
        "Swinging",
        "Obstacles",
        "Climbing",
        "FinalMessage"
    };

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;
        Debug.Log("[TutorialProgress] Ready.");
    }

    public static bool HasSeen(string triggerID)
    {
        return completedTutorials.Contains(triggerID);
    }

    public static bool CanShow(string triggerID)
    {
        if (completedTutorials.Contains(triggerID))
            return false;

        int currentIndex = System.Array.IndexOf(
            tutorialOrder, triggerID);

        if (currentIndex == -1)
            return true;

        for (int i = 0; i < currentIndex; i++)
        {
            if (!completedTutorials.Contains(tutorialOrder[i]))
                return false;
        }

        return true;
    }

    public static void MarkAsSeen(string triggerID)
    {
        completedTutorials.Add(triggerID);

        Debug.Log("[TutorialProgress] Completed: " + triggerID);
    }

    public static void ResetAllTutorials()
    {
        completedTutorials.Clear();

        Debug.Log("[TutorialProgress] All tutorials reset.");
    }
}
