using UnityEngine;

public class EnableController2D : MonoBehaviour
{
    [Header("Object to Enable/Disable")]
    [Tooltip("The GameObject that will be enabled or disabled")]
    public GameObject objectToToggle;

    [Header("Status Indicator")]
    [Tooltip("SpriteRenderer whose color reflects the state of the target. Green = disabled, Red = enabled")]
    public SpriteRenderer statusIndicator;

    [Tooltip("Color shown when the target object is DISABLED")]
    public Color disabledColor = Color.green;

    [Tooltip("Color shown when the target object is ENABLED")]
    public Color enabledColor = Color.red;

    [Header("Trigger Tags")]
    [Tooltip("If THIS object has this tag, entering will ENABLE the target")]
    public string enableTriggerTag = "EnableTrigger";

    [Tooltip("If THIS object has this tag, entering will DISABLE the target")]
    public string disableTriggerTag = "DisableTrigger";

    [Header("Startup State")]
    [Tooltip("What state the target should start in when the scene loads")]
    public bool startEnabled = true;

    void Awake()
    {
        if (objectToToggle == null)
        {
            Debug.LogError($"[{name}] objectToToggle is NOT assigned. Assign it in the Inspector.", this);
        }

        if (statusIndicator == null)
        {
            Debug.LogWarning($"[{name}] statusIndicator is NOT assigned. The color will not update.", this);
        }
    }

    void Start()
    {
        // Set the initial state of the target and the indicator
        if (objectToToggle != null)
        {
            objectToToggle.SetActive(startEnabled);
            Debug.Log($"[{name}] Started. My tag = '{tag}'. Target '{objectToToggle.name}' set active = {startEnabled}");
            UpdateIndicator(startEnabled);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"[{name}] OnTriggerEnter2D fired. Other = '{other.name}' (tag: '{other.tag}'). My tag = '{tag}'");

        if (CompareTag(enableTriggerTag))
        {
            Debug.Log($"[{name}] ENABLING {objectToToggle.name}");
            EnableTarget();
        }
        else if (CompareTag(disableTriggerTag))
        {
            Debug.Log($"[{name}] DISABLING {objectToToggle.name}");
            DisableTarget();
        }
        else
        {
            Debug.LogWarning($"[{name}] My tag '{tag}' does NOT match enableTriggerTag '{enableTriggerTag}' or disableTriggerTag '{disableTriggerTag}'");
        }
    }

    public void EnableTarget()
    {
        if (objectToToggle != null)
            objectToToggle.SetActive(true);

        UpdateIndicator(true);
    }

    public void DisableTarget()
    {
        if (objectToToggle != null)
            objectToToggle.SetActive(false);

        UpdateIndicator(false);
    }

    // Updates the status indicator sprite color based on the target state
    void UpdateIndicator(bool isEnabled)
    {
        if (statusIndicator == null) return;

        statusIndicator.color = isEnabled ? enabledColor : disabledColor;
    }

}