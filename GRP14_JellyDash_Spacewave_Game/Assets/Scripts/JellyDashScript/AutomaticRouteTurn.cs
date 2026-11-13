
using UnityEngine;

public class AutomaticRouteTurn : MonoBehaviour
{
    public enum RouteDirection
    {
        Left,
        Right
    }

    [Header("Route Direction")]
    [SerializeField] private RouteDirection direction;

    private void OnTriggerEnter2D(Collider2D other)
    {
        PlayerMovementJellyDash player =
            other.GetComponentInParent<PlayerMovementJellyDash>();

        if (player == null)
            return;

        float newDirection =
            direction == RouteDirection.Right ? 1f : -1f;

        player.SetAutomaticDirection(newDirection);

        Debug.Log("Route changed to: " + direction);
    }
}
