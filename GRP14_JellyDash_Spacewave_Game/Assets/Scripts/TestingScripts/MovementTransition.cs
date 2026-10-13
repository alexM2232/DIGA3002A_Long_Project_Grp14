using UnityEngine;

public class MovementTransition : MonoBehaviour
{
    
    [Header("Movement Mode")]
    [SerializeField]
    private FinalPlayerMovement.MovementMode targetMode;

    [Header("NPC")]
    [SerializeField]
    private npcMovement npc;

    private void OnTriggerEnter2D(Collider2D other)
    {
        FinalPlayerMovement movement =
            other.GetComponentInParent<FinalPlayerMovement>();

        if (movement == null)
            return;

        movement.SetMovementMode(targetMode);

        if (targetMode == FinalPlayerMovement.MovementMode.SpaceWaves && npc != null)
        {
            npc.activateNPC();
        }


        Debug.Log(
            "Movement transition triggered. New mode: " +
            targetMode
        );

    
    }
}