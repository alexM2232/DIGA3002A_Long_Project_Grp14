using UnityEngine;

public class CameraFollowTest : MonoBehaviour
{
    [Header("Player")]
    [SerializeField] private Transform player;

    [Header("Camera Settings")]
    [SerializeField] private float smoothSpeed = 5f;

    [Header("Camera Offset")]
    [SerializeField] private Vector3 offset =
        new Vector3(0f, 0f, -10f);

    private void LateUpdate()
    {
        if (player == null)
            return;

        Vector3 targetPosition =
            player.position + offset;

        transform.position =
            Vector3.Lerp(
                transform.position,
                targetPosition,
                smoothSpeed * Time.deltaTime
            );
    }
}