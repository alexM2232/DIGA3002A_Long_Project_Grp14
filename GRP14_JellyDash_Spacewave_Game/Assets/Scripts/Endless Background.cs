/*using UnityEngine;

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
}*/

/*using UnityEngine;

/// <summary>
/// Parallax / looping background that belongs to a specific environment.
/// Only scrolls while its environment is active. When the player leaves,
/// it freezes and (optionally) hides itself entirely.
/// </summary>
public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    [Tooltip("Camera to follow. Defaults to Camera.main if left empty.")]
    public Camera targetCamera;

    [Header("Environment")]
    [Tooltip("Which environment this background belongs to. Must match the id " +
             "used by EnvironmentZone triggers and EnvironmentManager.startingEnvironment.")]
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
    [Tooltip("Hide this background once the player leaves its environment.")]
    public bool disappearWhenPassed = true;

    [Tooltip("Optional one-way mode: hide once the camera passes this world-X. " +
             "Leave disabled to use environment-based hiding only.")]
    public bool useWorldXBoundary = false;
    public float passBoundaryX = 0f;

    [Tooltip("If true, disables the whole GameObject when hidden (cheapest). " +
             "If false, only disables renderers (keeps other scripts running).")]
    public bool deactivateGameObject = true;

    // ---------------------------------------------------------------
    private Transform[] layers;
    private Renderer[] renderers;
    private Vector3 lastCameraPos;
    private bool isActiveEnvironment = false;
    private bool isPassed = false;
    private bool hasInitialized = false;

    // ---------------------------------------------------------------
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);

        // Register with the manager so it can poke us directly on every
        // environment change — works even while SetActive(false).
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Register(this);
    }

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
            Debug.LogWarning($"EndlessBackground '{name}' needs at least 2 child objects to loop seamlessly.");

        lastCameraPos = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        hasInitialized = true;

        // Initial evaluation based on the manager's current state.
        EvaluateEnvironment();
    }

    void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Unregister(this);
    }

    // ---------------------------------------------------------------
    /// <summary>
    /// Single source of truth. Called by EnvironmentManager whenever the
    /// active environment changes. Safe to call on a disabled GameObject.
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized) return;

        bool shouldBeActive =
            EnvironmentManager.Instance != null &&
            EnvironmentManager.Instance.ActiveEnvironmentId == environmentId;

        if (shouldBeActive)
        {
            // (Re-)enter this environment: restore visibility, clear passed flag.
            isPassed = false;
            isActiveEnvironment = true;
            SetVisible(true);

            // Reset anchor so the first frame after waking doesn't compute a huge delta.
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else
        {
            // Leave this environment: stop scrolling, hide if configured.
            isActiveEnvironment = false;
            if (disappearWhenPassed)
                MarkPassed();
        }
    }

    /// <summary>Hides this background permanently (until ResetPassed).</summary>
    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        isActiveEnvironment = false;
        SetVisible(false);
    }

    /// <summary>Manually un-hide and re-evaluate against the manager.</summary>
    public void ResetPassed()
    {
        isPassed = false;
        isActiveEnvironment = false;   // force re-evaluation
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
        if (!hasInitialized || isPassed || !isActiveEnvironment || targetCamera == null)
            return;

        // Optional one-way world-X boundary.
        if (useWorldXBoundary && targetCamera.transform.position.x > passBoundaryX)
        {
            MarkPassed();
            return;
        }

        Vector3 camPos = targetCamera.transform.position;
        Vector3 delta = camPos - lastCameraPos;

        if (delta.x == 0f && delta.y == 0f)
            return;

        Vector3 moveDelta = new Vector3(
            delta.x * scrollSpeed,
            applyVerticalScroll ? delta.y * scrollSpeed : 0f,
            0f);

        foreach (Transform layer in layers)
        {
            layer.position += moveDelta;

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
*/

/*using UnityEngine;

public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Environment")]
    public string environmentId = "Environment1";

    [Header("Scroll Speed")]
    [Range(0f, 5f)]
    public float scrollSpeed = 0.5f;

    [Header("Background Setup")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical")]
    public bool applyVerticalScroll = false;

    [Header("Disappear When Passed")]
    public bool disappearWhenPassed = true;
    public bool useWorldXBoundary = false;
    public float passBoundaryX = 0f;
    public bool deactivateGameObject = true;

    // ---------------------------------------------------------------
    private Transform[] layers;
    private Renderer[] renderers;
    private Vector3 lastCameraPos;
    private bool isActiveEnvironment = false;
    private bool isPassed = false;
    private bool hasInitialized = false;

    // ---------------------------------------------------------------
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Register(this);
    }

    void Start()
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
            Debug.LogWarning($"EndlessBackground '{name}' needs at least 2 child objects.");

        lastCameraPos = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        hasInitialized = true;

        EvaluateEnvironment();
    }

    void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Unregister(this);
    }

    // ---------------------------------------------------------------
    /// <summary>
    /// Decides this background's state based on the manager's active
    /// AND previous environment ids:
    ///   active  → visible + scrolling
    ///   previous (just left) → hidden if disappearWhenPassed (passed)
    ///   anything else (upcoming) → visible + frozen
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized) return;
        if (EnvironmentManager.Instance == null) return;

        string active = EnvironmentManager.Instance.ActiveEnvironmentId;
        string previous = EnvironmentManager.Instance.PreviousEnvironmentId;

        if (environmentId == active)
        {
            // Entering / inside this environment
            isPassed = false;
            isActiveEnvironment = true;
            SetVisible(true);
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else if (disappearWhenPassed && environmentId == previous)
        {
            // Player just left this one heading forward → passed
            isActiveEnvironment = false;
            MarkPassed();
        }
        else
        {
            // Upcoming / not yet reached → stay visible, just frozen.
            isActiveEnvironment = false;
            // Do NOT MarkPassed() here — this was the bug.
            SetVisible(true);
        }
    }

    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        isActiveEnvironment = false;
        SetVisible(false);
    }

    public void ResetPassed()
    {
        isPassed = false;
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
        if (!hasInitialized || isPassed || !isActiveEnvironment || targetCamera == null)
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

public class EndlessBackground : MonoBehaviour
{
    [Header("Camera")]
    public Camera targetCamera;

    [Header("Environment")]
    public string environmentId = "Environment1";

    [Header("Scroll Speed")]
    [Range(0f, 5f)]
    public float scrollSpeed = 0.5f;

    [Header("Background Setup")]
    public float spriteWidth = 20f;
    public bool autoDetectWidth = true;

    [Header("Vertical")]
    public bool applyVerticalScroll = false;

    [Header("Disappear When Passed")]
    public bool disappearWhenPassed = true;
    public bool useWorldXBoundary = false;
    public float passBoundaryX = 0f;
    public bool deactivateGameObject = true;

    // ---------------------------------------------------------------
    private Transform[] layers;
    private Renderer[] renderers;
    private Vector3 lastCameraPos;
    private bool isActiveEnvironment = false;
    private bool isPassed = false;
    private bool hasInitialized = false;

    // ---------------------------------------------------------------
    void Awake()
    {
        renderers = GetComponentsInChildren<Renderer>(true);
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Register(this);
    }

    void Start()
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
            Debug.LogWarning($"EndlessBackground '{name}' needs at least 2 child objects.");

        lastCameraPos = targetCamera != null ? targetCamera.transform.position : Vector3.zero;
        hasInitialized = true;

        EvaluateEnvironment();
    }

    void OnDestroy()
    {
        if (EnvironmentManager.Instance != null)
            EnvironmentManager.Instance.Unregister(this);
    }

    // ---------------------------------------------------------------
    /// <summary>
    /// Three-state evaluation:
    ///   active   → visible + scrolling
    ///   previous → hidden (if disappearWhenPassed)  — "passed"
    ///   else     → visible + scrolling  (upcoming: still parallax + endless!)
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized || EnvironmentManager.Instance == null) return;

        string active = EnvironmentManager.Instance.ActiveEnvironmentId;
        string previous = EnvironmentManager.Instance.PreviousEnvironmentId;

        if (environmentId == active)
        {
            // Entering / inside this environment.
            isPassed = false;
            isActiveEnvironment = true;
            SetVisible(true);
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else if (disappearWhenPassed && environmentId == previous)
        {
            // Player just left this environment heading forward → passed.
            isActiveEnvironment = false;
            MarkPassed();
        }
        else
        {
            // Upcoming (or an environment we backtracked away from without
            // passing through the current transition) → keep it visible and
            // fully live so it parallaxes and wraps like normal.
            isActiveEnvironment = false;
            isPassed = false;
            SetVisible(true);
            // Reset anchor so it doesn't jump on the frame we re-show it.
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
    }

    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        isActiveEnvironment = false;
        SetVisible(false);
    }

    public void ResetPassed()
    {
        isPassed = false;
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
        // Only stop scrolling if:
        //   - not initialized
        //   - passed / hidden  (player is done with it)
        //   - no camera
        // "Not the active environment" NO LONGER stops scrolling.
        if (!hasInitialized || isPassed || targetCamera == null)
            return;

        // Optional one-way world-X boundary.
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