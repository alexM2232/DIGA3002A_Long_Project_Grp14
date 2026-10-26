/*using UnityEngine;

/// <summary>
/// Parallax / looping background that belongs to a specific environment.
///
/// Lifecycle:
///   DORMANT  (never triggered)  → visible, frozen
///   ACTIVE   (triggered / starting env) → visible, scrolling, wrapping
///   PASSED   (left forward)     → hidden forever (one-way latch)
///
/// Scrolling is gated by hasBeenActivated, which only becomes true when the
/// manager activates this environment (either at startup via
/// startingEnvironment, or via a trigger firing).
/// </summary>
public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to follow. Defaults to Camera.main if left empty.")]
    public Camera targetCamera;

    [Header("Environment")]
    [Tooltip("Which environment this background belongs to. Must match the id used by " +
             "EnvironmentZone triggers and EnvironmentManager.startingEnvironment.")]
    public string environmentId = "Environment1";

    [Header("Scroll Speed")]
    [Range(0f, 5f)]
    [Tooltip("0 = locked to world, 0.5 = half-speed parallax, 1 = static on screen, >1 = foreground parallax.")]
    public float scrollSpeed = 0.5f;

    [Header("Background Setup")]
    [Tooltip("Width of a single background sprite in world units. Auto-detected if enabled.")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical")]
    [Tooltip("If true, applies the same scrollSpeed multiplier on the Y axis too.")]
    public bool applyVerticalScroll = false;

    [Header("Disappear When Passed")]
    [Tooltip("Hide this background permanently once the player leaves its environment going forward.")]
    public bool disappearWhenPassed = true;

    [Tooltip("Optional one-way mode: hide once the camera passes this world-X.")]
    public bool useWorldXBoundary = false;
    public float passBoundaryX = 0f;

    [Tooltip("If true, disables the whole GameObject when hidden (cheapest). " +
             "If false, only disables renderers (keeps other scripts running).")]
    public bool deactivateGameObject = true;

    // ---------------------------------------------------------------
    private Transform[] layers;
    private Renderer[] renderers;
    private Vector3 lastCameraPos;
    private bool isPassed = false;
    private bool hasBeenActivated = false;   // gate: only true once manager activates this env
    private bool hasInitialized = false;

    // ---------------------------------------------------------------
    void Awake()
    {
        // Initialize everything here so it works even if the GameObject
        // starts inactive (Start() would never run in that case).
        renderers = GetComponentsInChildren<Renderer>(true);

        if (targetCamera == null) targetCamera = Camera.main;

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
            Debug.LogWarning($"EndlessBackground '{name}' needs at least 2 child objects to loop seamlessly.");

        lastCameraPos = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        hasInitialized = true;
    }

    void Start()
    {
        // Register AFTER all Awake()s have run, so EnvironmentManager.Instance
        // is guaranteed to exist regardless of script execution order.
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Register(this);

        EvaluateEnvironment();
    }

    void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Unregister(this);
    }

    // ---------------------------------------------------------------
    /// <summary>
    /// Called by EnvironmentManager whenever the active environment changes.
    ///
    /// State machine:
    ///   active (triggered or starting env) → hasBeenActivated=true, visible, scrolls
    ///   previous AND we were activated     → passed, hidden forever
    ///   otherwise (never triggered)        → visible, frozen (dormant)
    ///
    /// Once passed, this method returns early forever (one-way latch).
    /// Only ResetPassed() can revive it.
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized || EnvironmentManager.Instance == null) return;

        // One-way latch: a passed background never comes back on its own.
        if (isPassed) return;

        string active = EnvironmentManager.Instance.ActiveEnvironmentId;
        string previous = EnvironmentManager.Instance.PreviousEnvironmentId;

        if (!string.IsNullOrEmpty(active) && environmentId == active)
        {
            // The starting environment lands here on frame one because
            // EnvironmentManager.startingEnvironment == "Environment1".
            // Triggered environments land here when their zone fires.
            hasBeenActivated = true;
            SetVisible(true);
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else if (disappearWhenPassed && hasBeenActivated && environmentId == previous)
        {
            // Player just left this environment going forward → passed forever.
            MarkPassed();
        }
        else
        {
            // Dormant: upcoming, or an environment we've never entered.
            // Visible but frozen — hasBeenActivated stays false.
            SetVisible(true);
        }
    }

    /// <summary>Permanently hides this background. Only ResetPassed() reverses it.</summary>
    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        hasBeenActivated = false;
        SetVisible(false);
    }

    /// <summary>Manual escape hatch (e.g. level restart).</summary>
    public void ResetPassed()
    {
        isPassed = false;
        hasBeenActivated = false;
        EvaluateEnvironment();
    }

    private void SetVisible(bool visible)
    {
        if (deactivateGameObject)
        {
            if (gameObject.activeSelf != visible)
                gameObject.SetActive(visible);
        }
        else
        {
            foreach (var r in renderers)
                if (r != null) r.enabled = visible;
        }
    }

    // ---------------------------------------------------------------
    void LateUpdate()
    {
        // Late-resolve camera in case it was spawned after this object.
        if (targetCamera == null) targetCamera = Camera.main;

        // Scrolling requires ALL of:
        //   - initialized
        //   - not passed
        //   - has been activated by the manager (this is the trigger gate)
        //   - camera present
        if (!hasInitialized || isPassed || !hasBeenActivated || targetCamera == null)
            return;

        if (useWorldXBoundary && targetCamera.transform.position.x > passBoundaryX)
        {
            MarkPassed();
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - lastCameraPos;
        if (delta.x == 0f && delta.y == 0f) return;

        Vector3 moveDelta = new Vector3(
            delta.x * scrollSpeed,
            applyVerticalScroll ? delta.y * scrollSpeed : 0f,
            0f);

        foreach (Transform layer in layers)
        {
            layer.position += moveDelta;

            float relativeX = layer.position.x - camPos.x;
            if (relativeX < -spriteWidth)
                layer.position = new Vector3(layer.position.x + spriteWidth * 2f, layer.position.y, layer.position.z);
            else if (relativeX > spriteWidth)
                layer.position = new Vector3(layer.position.x - spriteWidth * 2f, layer.position.y, layer.position.z);
        }

        lastCameraPos = camPos;
    }
}*/

using UnityEngine;

/// <summary>
/// Parallax / looping background that belongs to a specific environment.
///
/// Lifecycle:
///   DORMANT  (never triggered)          → visible, frozen
///   ACTIVE   (triggered / starting env) → visible, scrolling, wrapping
///   PASSED   (left forward)             → visible, frozen in place (no longer scrolls)
///
/// Scrolling is gated by hasBeenActivated, which becomes true only when the
/// manager activates this environment (startup env or trigger).
/// Once passed, the background stays visible and simply stops moving.
/// </summary>
public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to follow. Defaults to Camera.main if left empty.")]
    public Camera targetCamera;

    [Header("Environment")]
    [Tooltip("Which environment this background belongs to. Must match the id used by " +
             "EnvironmentZone triggers and EnvironmentManager.startingEnvironment.")]
    public string environmentId = "Environment1";

    [Header("Scroll Speed")]
    [Range(0f, 5f)]
    [Tooltip("0 = locked to world, 0.5 = half-speed parallax, 1 = static on screen, >1 = foreground parallax.")]
    public float scrollSpeed = 0.5f;

    [Header("Background Setup")]
    [Tooltip("Width of a single background sprite in world units. Auto-detected if enabled.")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical")]
    [Tooltip("If true, applies the same scrollSpeed multiplier on the Y axis too.")]
    public bool applyVerticalScroll = false;

    [Header("Stop When Passed")]
    [Tooltip("When the player leaves this environment going forward, stop scrolling it. " +
             "The background remains visible and stays exactly where it was.")]
    public bool stopWhenPassed = true;

    [Tooltip("Optional one-way mode: stop once the camera passes this world-X.")]
    public bool useWorldXBoundary = false;
    public float passBoundaryX = 0f;

    // ---------------------------------------------------------------
    private Transform[] layers;
    private Vector3 lastCameraPos;
    private bool isPassed = false;           // "no longer scrolls" — NOT "hidden"
    private bool hasBeenActivated = false;   // gate: true once manager activates this env
    private bool hasInitialized = false;

    // ---------------------------------------------------------------
    void Awake()
    {
        if (targetCamera == null) targetCamera = Camera.main;

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
            Debug.LogWarning($"EndlessBackground '{name}' needs at least 2 child objects to loop seamlessly.");

        lastCameraPos = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        hasInitialized = true;
    }

    void Start()
    {
        // Register AFTER all Awake()s have run so the singleton exists.
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Register(this);

        EvaluateEnvironment();
    }

    void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Unregister(this);
    }

    // ---------------------------------------------------------------
    /// <summary>
    /// State machine:
    ///   active (triggered or starting env) → hasBeenActivated=true, scrolls
    ///   previous AND we were activated     → passed: stops scrolling, stays visible
    ///   otherwise (never triggered)        → dormant: stays visible, frozen
    ///
    /// Once passed, this method returns early forever (one-way latch).
    /// Only ResetPassed() can revive it.
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized || EnvironmentManager.Instance == null) return;

        // One-way latch: a passed background never re-activates on its own.
        if (isPassed) return;

        string active = EnvironmentManager.Instance.ActiveEnvironmentId;
        string previous = EnvironmentManager.Instance.PreviousEnvironmentId;

        if (!string.IsNullOrEmpty(active) && environmentId == active)
        {
            // Starting env on frame one, or a triggered env.
            hasBeenActivated = true;
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else if (stopWhenPassed && hasBeenActivated && environmentId == previous)
        {
            // Player just left this environment going forward → freeze in place.
            MarkPassed();
        }
        else
        {
            // Dormant: upcoming, or an environment we've never entered.
            // Stays visible and frozen — hasBeenActivated stays false.
        }
    }

    /// <summary>Stops this background from scrolling. It stays visible and in place.</summary>
    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        hasBeenActivated = false;
        // No hiding — the object simply stops moving because LateUpdate bails on isPassed.
    }

    /// <summary>Manual escape hatch (e.g. level restart).</summary>
    public void ResetPassed()
    {
        isPassed = false;
        hasBeenActivated = false;
        EvaluateEnvironment();
    }

    // ---------------------------------------------------------------
    void LateUpdate()
    {
        // Late-resolve camera in case it was spawned after this object.
        if (targetCamera == null) targetCamera = Camera.main;

        // Scrolling requires ALL of:
        //   - initialized
        //   - NOT passed (this is what freezes a passed background in place)
        //   - has been activated by the manager
        //   - camera present
        if (!hasInitialized || isPassed || !hasBeenActivated || targetCamera == null)
            return;

        if (useWorldXBoundary && targetCamera.transform.position.x > passBoundaryX)
        {
            MarkPassed();
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - lastCameraPos;
        if (delta.x == 0f && delta.y == 0f) return;

        Vector3 moveDelta = new Vector3(
            delta.x * scrollSpeed,
            applyVerticalScroll ? delta.y * scrollSpeed : 0f,
            0f);

        foreach (Transform layer in layers)
        {
            layer.position += moveDelta;

            float relativeX = layer.position.x - camPos.x;
            if (relativeX < -spriteWidth)
                layer.position = new Vector3(layer.position.x + spriteWidth * 2f, layer.position.y, layer.position.z);
            else if (relativeX > spriteWidth)
                layer.position = new Vector3(layer.position.x - spriteWidth * 2f, layer.position.y, layer.position.z);
        }

        lastCameraPos = camPos;
    }
}