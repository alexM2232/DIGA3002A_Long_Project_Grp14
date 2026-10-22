/*using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to follow. Defaults to Camera.main if left empty.")]
    public Camera targetCamera;

    [Header("Background Setup")]
    [Tooltip("Width of a single background sprite in world units. Auto-detected if enabled.")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical Position")]
    [Tooltip("If true, the background will follow the camera vertically as well (useful for vertical scrolling levels).")]
    public bool followVertically = false;

    private Transform[] layers;
    private float halfWidth;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        // Auto-detect the sprite width
        if (autoDetectWidth)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                spriteWidth = sr.bounds.size.x;
        }

        halfWidth = spriteWidth;

        // Grab the child layers (need at least 2)
        layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            layers[i] = transform.GetChild(i);

        if (layers.Length < 2)
            Debug.LogWarning("EndlessBackground needs at least 2 child objects to loop seamlessly.");
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 camPos = targetCamera.transform.position;

        foreach (Transform layer in layers)
        {
            // Wrap the layer so it stays within one sprite-width of the camera
            float relativeX = layer.position.x - camPos.x;

            if (relativeX < -halfWidth)
            {
                // Layer is too far left — push it right by 2 widths
                layer.position = new Vector3(
                    layer.position.x + spriteWidth * 2f,
                    layer.position.y,
                    layer.position.z);
            }
            else if (relativeX > halfWidth)
            {
                // Layer is too far right — push it left by 2 widths
                layer.position = new Vector3(
                    layer.position.x - spriteWidth * 2f,
                    layer.position.y,
                    layer.position.z);
            }

            // Optional: keep the background vertically aligned with the camera
            if (followVertically)
            {
                layer.position = new Vector3(
                    layer.position.x,
                    camPos.y,
                    layer.position.z);
            }
        }
    }
}*/

using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to follow. Defaults to Camera.main if left empty.")]
    public Camera targetCamera;

    [Header("Scroll Speed")]
    [Tooltip("Multiplier for how fast the background moves relative to the camera.\n0   = background stays still (locked to world)\n0.5 = moves at half the camera's speed (classic parallax)\n1   = moves exactly with the camera (appears static on screen)\n>1  = moves faster than the camera (foreground parallax)")]
    public float scrollSpeed = 0.5f;

    [Header("Background Setup")]
    [Tooltip("Width of a single background sprite in world units. Auto-detected if enabled.")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical")]
    [Tooltip("If true, applies the same multiplier on the Y axis too (for vertical levels).")]
    public bool applyVerticalScroll = false;

    private Transform[] layers;
    private Vector3 lastCameraPos;

    void Start()
    {
        if (targetCamera == null)
            targetCamera = Camera.main;

        if (autoDetectWidth)
        {
            SpriteRenderer sr = GetComponentInChildren<SpriteRenderer>();
            if (sr != null && sr.sprite != null)
                spriteWidth = sr.bounds.size.x;
        }

        layers = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
            layers[i] = transform.GetChild(i);

        if (layers.Length < 2)
            Debug.LogWarning("EndlessBackground needs at least 2 child objects to loop seamlessly.");

        lastCameraPos = targetCamera.transform.position;
    }

    void LateUpdate()
    {
        if (targetCamera == null) return;

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - lastCameraPos;

        // No camera movement this frame → nothing to do
        if (delta.x == 0f && delta.y == 0f)
            return;

        // Scale the camera's movement by scrollSpeed
        Vector3 moveDelta = new Vector3(
            delta.x * scrollSpeed,
            applyVerticalScroll ? delta.y * scrollSpeed : 0f,
            0f);

        foreach (Transform layer in layers)
        {
            layer.position += moveDelta;

            // Wrap around the camera to keep the illusion infinite
            float relativeX = layer.position.x - camPos.x;

            if (relativeX < -spriteWidth)
            {
                layer.position = new Vector3(
                    layer.position.x + spriteWidth * 2f,
                    layer.position.y,
                    layer.position.z);
            }
            else if (relativeX > spriteWidth)
            {
                layer.position = new Vector3(
                    layer.position.x - spriteWidth * 2f,
                    layer.position.y,
                    layer.position.z);
            }
        }

        lastCameraPos = camPos;
    }
}