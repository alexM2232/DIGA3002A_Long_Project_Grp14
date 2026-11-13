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

   [Header("Mistakes")]
   [Range(0f,1f)] public float mistakeChance = 0.1f;   // Chance to misjudge when it needs to change course
   public float mistakeHoldTime = 0.3f;                 // How long it commits to a mistake before it can correct
   [Range(0f,1f)] public float wobbleChance = 0.02f;    // Chance of a pointless direction flip when the path is clear
   public float reactionDelayMax = 0.25f;     // Max extra delay before jumping at a reckless setting
   [Range(0f,1f)] public float riskyJumpChance = 0.5f;   // At full recklessness, chance to ignore an unsafe arc
}

