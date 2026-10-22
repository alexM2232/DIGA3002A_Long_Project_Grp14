using UnityEngine;

public class JellySpikes : MonoBehaviour
{
    [Header("Jelly Settings")]
    [Tooltip("How fast the jelly wobbles.")]
    public float speed = 5f;

    [Tooltip("How much the jelly stretches and squishes. 0.1 = 10% change.")]
    [Range(0f, 0.5f)]
    public float intensity = 0.15f;

    [Header("Pivot Adjustment")]
    [Tooltip("If the spikes sink into the ground, adjust this so the bottom stays put.")]
    public float yOffset = 0f;

    private Vector3 _initialScale;
    private Vector3 _initialPosition;

    void Start()
    {
        // Store the original scale and position of the sprite
        _initialScale = transform.localScale;
        _initialPosition = transform.localPosition;
    }

    void Update()
    {
        // Calculate the Sine wave based on time
        // Mathf.Sin returns a value between -1 and 1
        float wave = Mathf.Sin(Time.time * speed) * intensity;

        // Apply the wave to the X and Y axes inversely.
        // If Y goes up (+), X goes down (-). This preserves the "volume" of the object.
        float scaleX = _initialScale.x * (1f - wave);
        float scaleY = _initialScale.y * (1f + wave);

        // Apply the new scale
        transform.localScale = new Vector3(scaleX, scaleY, _initialScale.z);

        // --- Pivot Fix ---
        // By default, Unity scales from the center. If your spikes are sitting on the ground,
        // scaling them will make them sink into the floor. 
        // This calculation keeps the bottom edge pinned to the ground.
        float heightDifference = _initialScale.y - scaleY;
        transform.localPosition = new Vector3(
            _initialPosition.x,
            _initialPosition.y - (heightDifference / 2f) + yOffset,
            _initialPosition.z
        );
    }
}