using UnityEngine;

public class PendulumSwing : MonoBehaviour
{
    [Header("Swing Settings")]
    [Tooltip("Maximum angle of the swing in degrees (from vertical)")]
    [Range(0f, 180f)]
    public float swingAngle = 45f;

    [Tooltip("Speed of the swing (higher = faster)")]
    [Range(0f, 10f)]
    public float swingSpeed = 2f;

    [Tooltip("Starting phase offset in degrees (changes where in the swing the object starts)")]
    [Range(0f, 360f)]
    public float startPhase = 0f;

    [Tooltip("Randomize the starting phase when the scene starts")]
    public bool randomizeStartPhase = false;

    [Header("Rotation Axis")]
    [Tooltip("The local axis to rotate around (Z for 2D sprites)")]
    public Vector3 rotationAxis = Vector3.forward;

    [Header("Pivot (Optional)")]
    [Tooltip("Leave empty to pivot around the object's own origin. Assign a parent/empty at the pivot point for a hanging pendulum.")]
    public Transform pivotTransform;

    // Internal state
    private Quaternion initialLocalRotation;
    private float currentTime;

    void Start()
    {
        // Store the starting rotation so the swing is relative to it
        initialLocalRotation = transform.localRotation;

        // Optionally randomize the phase so multiple pendulums don't sync up
        if (randomizeStartPhase)
        {
            startPhase = Random.Range(0f, 360f);
        }

        // Initialize time based on phase
        currentTime = startPhase * Mathf.Deg2Rad;
    }

    void Update()
    {
        // Use sine wave for smooth, natural pendulum motion
        float angleOffset = Mathf.Sin(currentTime) * swingAngle;

        // Apply the rotation relative to the initial rotation
        Quaternion swingRotation = Quaternion.AngleAxis(angleOffset, rotationAxis);

        if (pivotTransform != null)
        {
            // Rotate around the pivot point (for a hanging pendulum)
            transform.localRotation = initialLocalRotation * swingRotation;
        }
        else
        {
            // Rotate around the object's own origin
            transform.localRotation = initialLocalRotation * swingRotation;
        }

        // Advance time by speed
        currentTime += swingSpeed * Time.deltaTime;

        // Keep time from growing too large (optional, prevents float precision issues)
        if (currentTime > Mathf.PI * 2f)
        {
            currentTime -= Mathf.PI * 2f;
        }
    }

    // Optional: preview the swing in the editor when not in play mode
    void OnValidate()
    {
        if (!Application.isPlaying && pivotTransform != null)
        {
            // Nothing extra needed - just ensures values are valid
        }
    }
}