using UnityEngine;

public class CameraFollow : MonoBehaviour
{
    public Transform target;        // The player to follow
    public float smoothSpeed = 5f;  // How smoothly the camera follows
    public Vector3 offset = new Vector3(0f, 0f, -10f); // Camera offset from player

    void LateUpdate()
    {
        if (target == null) return;

        Vector3 desiredPosition = target.position + offset;
        Vector3 smoothedPosition = Vector3.Lerp(transform.position, desiredPosition, smoothSpeed * Time.deltaTime);
        transform.position = smoothedPosition;
    }
}