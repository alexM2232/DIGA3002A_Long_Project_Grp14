using UnityEngine;

public class MovementTransition : MonoBehaviour
{
    [Header("Movement Mode")]
    [SerializeField]
    private FinalPlayerMovement.MovementMode targetMode;

    private void OnTriggerEnter2D(Collider2D other)
    {
        FinalPlayerMovement movement =
            other.GetComponent<FinalPlayerMovement>();

        if (movement == null)
            return;

        movement.SetMovementMode(targetMode);

        Debug.Log(
            "Movement transition triggered. New mode: " +
            targetMode
        );
    }
}