using UnityEngine;

[CreateAssetMenu(fileName = "NPCProfile", menuName = "NPC Profile")]
public class npcProfile : ScriptableObject 
{
   [Header("Movement")]
   public float speed = 4f;
   public float radius = 0.25f; //collision detection radius

   [Header("Perception")]
   public LayerMask obstacleMask;
   public float safetyMargin = 0.1f; //expands npcs prediction radius
   public float stepTime = 0.12f; //how far ahead each moves travels in time
   [Range(2,10)] public int depth = 7; //how many future steps evaluated
   [Range(0f,1f)] public float recklessness; //Risky paths

   [Header("Behaviour")]
   public float yWeight = 1f; //how strongly they want to stay near target Y
   public float switchPenalty = 0.5f; //how much the NPC switches up and down
   public float decisionInterval = 0.03f;//reaction speed
}
