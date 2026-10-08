/*using System.Collections;
using UnityEngine;


public class EnableControllerAuto : MonoBehaviour
{
    [Header("Objects to Toggle")]
    [Tooltip("This GameObject will be ENABLED during the first phase, then DISABLED during the second phase.")]
    public GameObject objectToEnable;

    [Tooltip("This GameObject will be DISABLED during the first phase, then ENABLED during the second phase.")]
    public GameObject objectToDisable;

    [Header("Timing")]
    [Tooltip("How long (in seconds) each phase lasts before swapping.")]
    public float duration = 5f;

    [Header("Status Indicator")]
    [Tooltip("SpriteRenderer whose color reflects the state of objectToEnable. Red = enabled, Green = disabled.")]
    public SpriteRenderer statusIndicator;

    [Tooltip("Color shown when objectToEnable is DISABLED")]
    public Color disabledColor = Color.green;

    [Tooltip("Color shown when objectToEnable is ENABLED")]
    public Color enabledColor = Color.red;

    [Header("Startup State")]
    [Tooltip("If true, objectToEnable starts active and objectToDisable starts inactive.")]
    public bool startEnabled = true;

    private Coroutine loopRoutine;

    void Awake()
    {
        if (objectToEnable == null)
            Debug.LogError($"[{name}] objectToEnable is NOT assigned. Assign it in the Inspector.", this);

        if (objectToDisable == null)
            Debug.LogError($"[{name}] objectToDisable is NOT assigned. Assign it in the Inspector.", this);

        if (statusIndicator == null)
            Debug.LogWarning($"[{name}] statusIndicator is NOT assigned. The color will not update.", this);
    }

    void Start()
    {
        ApplyState(startEnabled);
        Debug.Log($"[{name}] Started. Initial state: objectToEnable active = {startEnabled}");

        loopRoutine = StartCoroutine(LoopRoutine());
    }

    private IEnumerator LoopRoutine()
    {
        bool isEnabled = startEnabled;

        while (true)
        {
            // Wait for the current phase's duration
            yield return new WaitForSeconds(duration);

            // Flip the state
            isEnabled = !isEnabled;
            ApplyState(isEnabled);
        }
    }

    // Applies the given state to both objects and updates the indicator.
    // isEnabled == true  -> objectToEnable ON,  objectToDisable OFF
    // isEnabled == false -> objectToEnable OFF, objectToDisable ON
    void ApplyState(bool isEnabled)
    {
        if (objectToEnable != null)
            objectToEnable.SetActive(isEnabled);

        if (objectToDisable != null)
            objectToDisable.SetActive(!isEnabled);

        UpdateIndicator(isEnabled);
    }

    // Updates the status indicator sprite color based on the objectToEnable state
    void UpdateIndicator(bool isEnabled)
    {
        if (statusIndicator == null) return;

        statusIndicator.color = isEnabled ? enabledColor : disabledColor;
    }

    // Optional: stop the loop if this component is disabled
    void OnDisable()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
    }
}
*/

using System.Collections;
using UnityEngine;

public class EnableControllerAuto : MonoBehaviour
{
    [Header("Objects to Toggle")]
    [Tooltip("This GameObject will be ENABLED during the first phase, then DISABLED during the second phase.")]
    public GameObject objectToEnable;

    [Tooltip("This GameObject will be DISABLED during the first phase, then ENABLED during the second phase.")]
    public GameObject objectToDisable;

    [Header("Timing")]
    [Tooltip("How long (in seconds) each phase lasts before swapping.")]
    public float duration = 5f;

    [Header("Status Indicator - Object A (objectToEnable)")]
    [Tooltip("SpriteRenderer whose color reflects the state of objectToEnable.")]
    public SpriteRenderer statusIndicatorA;

    [Tooltip("Color shown when objectToEnable is ENABLED")]
    public Color enabledColorA = Color.red;

    [Tooltip("Color shown when objectToEnable is DISABLED")]
    public Color disabledColorA = Color.green;

    [Header("Status Indicator - Object B (objectToDisable)")]
    [Tooltip("SpriteRenderer whose color reflects the state of objectToDisable.")]
    public SpriteRenderer statusIndicatorB;

    [Tooltip("Color shown when objectToDisable is ENABLED")]
    public Color enabledColorB = Color.red;

    [Tooltip("Color shown when objectToDisable is DISABLED")]
    public Color disabledColorB = Color.green;

    [Header("Startup State")]
    [Tooltip("If true, objectToEnable starts active and objectToDisable starts inactive.")]
    public bool startEnabled = true;

    private Coroutine loopRoutine;

    void Awake()
    {
        if (objectToEnable == null)
            Debug.LogError($"[{name}] objectToEnable is NOT assigned. Assign it in the Inspector.", this);

        if (objectToDisable == null)
            Debug.LogError($"[{name}] objectToDisable is NOT assigned. Assign it in the Inspector.", this);

        if (statusIndicatorA == null)
            Debug.LogWarning($"[{name}] statusIndicatorA is NOT assigned. Its color will not update.", this);

        if (statusIndicatorB == null)
            Debug.LogWarning($"[{name}] statusIndicatorB is NOT assigned. Its color will not update.", this);
    }

    void Start()
    {
        ApplyState(startEnabled);
        Debug.Log($"[{name}] Started. Initial state: objectToEnable active = {startEnabled}");

        loopRoutine = StartCoroutine(LoopRoutine());
    }

    private IEnumerator LoopRoutine()
    {
        bool isEnabled = startEnabled;

        while (true)
        {
            // Wait for the current phase's duration
            yield return new WaitForSeconds(duration);

            // Flip the state
            isEnabled = !isEnabled;
            ApplyState(isEnabled);
        }
    }

    // Applies the given state to both objects and updates both indicators.
    // isEnabled == true  -> objectToEnable ON,  objectToDisable OFF
    // isEnabled == false -> objectToEnable OFF, objectToDisable ON
    void ApplyState(bool isEnabled)
    {
        if (objectToEnable != null)
            objectToEnable.SetActive(isEnabled);

        if (objectToDisable != null)
            objectToDisable.SetActive(!isEnabled);

        UpdateIndicators(isEnabled);
    }

    // Updates both status indicator sprite colors based on the current state.
    void UpdateIndicators(bool isEnabled)
    {
        // Indicator A reflects objectToEnable
        if (statusIndicatorA != null)
            statusIndicatorA.color = isEnabled ? enabledColorA : disabledColorA;

        // Indicator B reflects objectToDisable (opposite of objectToEnable)
        if (statusIndicatorB != null)
            statusIndicatorB.color = isEnabled ? disabledColorB : enabledColorB;
    }

    // Optional: stop the loop if this component is disabled
    void OnDisable()
    {
        if (loopRoutine != null)
        {
            StopCoroutine(loopRoutine);
            loopRoutine = null;
        }
    }
}