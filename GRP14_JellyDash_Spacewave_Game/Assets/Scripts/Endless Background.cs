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
    ///   previous → hidden permanently ("passed") — one-way latch
    ///   else     → visible + scrolling (upcoming)
    /// Once passed, this method does nothing. Only ResetPassed() can revive it.
    /// </summary>
    public void EvaluateEnvironment()
    {
        if (!hasInitialized || EnvironmentManager.Instance == null) return;

        // ONE-WAY LATCH: a passed background never comes back on its own.
        if (isPassed) return;

        string active = EnvironmentManager.Instance.ActiveEnvironmentId;
        string previous = EnvironmentManager.Instance.PreviousEnvironmentId;

        if (environmentId == active)
        {
            // Entering / inside this environment.
            isActiveEnvironment = true;
            SetVisible(true);
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
        else if (disappearWhenPassed && environmentId == previous)
        {
            // Player just left this environment heading forward → passed forever.
            isActiveEnvironment = false;
            MarkPassed();
        }
        else
        {
            // Upcoming (or an environment we backtracked away from).
            // Keep it fully live so it parallaxes and wraps like normal.
            isActiveEnvironment = false;
            SetVisible(true);
            if (targetCamera != null)
                lastCameraPos = targetCamera.transform.position;
        }
    }

    /// <summary>Permanently hides this background. Only ResetPassed() reverses it.</summary>
    public void MarkPassed()
    {
        if (isPassed) return;
        isPassed = true;
        isActiveEnvironment = false;
        SetVisible(false);
    }

    /// <summary>Manual escape hatch — revives a passed background. Use with care.</summary>
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
        // Stops if passed (latched), uninitialized, or no camera.
        // Upcoming backgrounds still scroll so they parallax into view.
        if (!hasInitialized || isPassed || targetCamera == null)
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