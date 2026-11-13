
using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class MovementTransition : MonoBehaviour
{
    [Header("Movement Mode")]
    [SerializeField]
    private FinalPlayerMovement.MovementMode targetMode;

    [Header("Player Detection")]
    [SerializeField]
    private string playerTag = "Player";

    [Header("NPC")]
    [SerializeField]
    private npcMovement npc;

    private Collider2D transitionCollider;

    private void Awake()
    {
        transitionCollider = GetComponent<Collider2D>();

        if (!transitionCollider.isTrigger)
        {
            Debug.LogWarning(
                "MovementTransition: Collider must have Is Trigger enabled.",
                this
            );
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Only accept the main Player object.
        // Child colliders such as SwingZone are ignored.
        if (!other.CompareTag(playerTag))
            return;

        // Get FinalPlayerMovement directly from the object
        // carrying the Player tag.
        FinalPlayerMovement movement =
            other.GetComponent<FinalPlayerMovement>();

        if (movement == null)
        {
            Debug.LogWarning(
                "MovementTransition: FinalPlayerMovement was not found " +
                "on the main Player object.",
                this
            );

            return;
        }

        Debug.Log(
            "[MovementTransition] Main Player reached transition: " +
            gameObject.name +
            " | Target Mode: " + targetMode
        );

        movement.SetMovementMode(targetMode);

        if (npc != null)
        {
            if (targetMode == FinalPlayerMovement.MovementMode.SpaceWaves)
                npc.activateNPC();
            else
                npc.deactivateNPC();
        }
    }
}
