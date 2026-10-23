/*using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central authority for which environment the player is currently in.
/// Backgrounds register themselves and are poked directly whenever the
/// active environment changes — this works even for GameObjects that have
/// been hidden with SetActive(false), because direct C# method calls on
/// disabled components still execute.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Startup")]
    [Tooltip("Environment the player starts in when the scene loads.")]
    public string startingEnvironment = "Environment1";

    /// <summary>Id of the environment the player is currently inside.</summary>
    public string ActiveEnvironmentId { get; private set; }

    /// <summary>Fired with the new environment id whenever it changes.</summary>
    public event Action<string> OnEnvironmentChanged;

    // Registered backgrounds (may include disabled GameObjects).
    private readonly List<EndlessBackground> backgrounds = new List<EndlessBackground>();

    // ---------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        ActiveEnvironmentId = startingEnvironment;
    }

    /// <summary>Called by EndlessBackground.Awake(). Safe to call multiple times.</summary>
    public void Register(EndlessBackground bg)
    {
        if (bg != null && !backgrounds.Contains(bg))
            backgrounds.Add(bg);
    }

    /// <summary>Called by EndlessBackground.OnDestroy().</summary>
    public void Unregister(EndlessBackground bg)
    {
        backgrounds.Remove(bg);
    }

    /// <summary>Switch the active environment. Call from EnvironmentZone triggers.</summary>
    public void SetEnvironment(string environmentId)
    {
        if (string.IsNullOrEmpty(environmentId)) return;
        if (environmentId == ActiveEnvironmentId) return;

        ActiveEnvironmentId = environmentId;
        Debug.Log($"[EnvironmentManager] Active environment → '{environmentId}'");

        // Event listeners (optional extras, UI, audio, etc.)
        OnEnvironmentChanged?.Invoke(environmentId);

        // Directly notify every registered background, including disabled ones.
        // Iterate backwards so we can safely remove destroyed entries.
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            var bg = backgrounds[i];
            if (bg == null)
            {
                backgrounds.RemoveAt(i);
                continue;
            }
            bg.EvaluateEnvironment();
        }
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}*/

using System;
using System.Collections.Generic;
using UnityEngine;

public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Startup")]
    public string startingEnvironment = "Environment1";

    public string ActiveEnvironmentId { get; private set; }

    /// <summary>The environment the player was in before the current one (or null).</summary>
    public string PreviousEnvironmentId { get; private set; }

    public event Action<string> OnEnvironmentChanged;

    private readonly List<EndlessBackground> backgrounds = new List<EndlessBackground>();

    // ---------------------------------------------------------------
    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        ActiveEnvironmentId = startingEnvironment;
        PreviousEnvironmentId = null;
    }

    public void Register(EndlessBackground bg)
    {
        if (bg != null && !backgrounds.Contains(bg)) backgrounds.Add(bg);
    }

    public void Unregister(EndlessBackground bg) => backgrounds.Remove(bg);

    public void SetEnvironment(string environmentId)
    {
        if (string.IsNullOrEmpty(environmentId)) return;
        if (environmentId == ActiveEnvironmentId) return;

        PreviousEnvironmentId = ActiveEnvironmentId;
        ActiveEnvironmentId = environmentId;
        Debug.Log($"[EnvironmentManager] '{PreviousEnvironmentId}' → '{ActiveEnvironmentId}'");

        OnEnvironmentChanged?.Invoke(environmentId);

        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            var bg = backgrounds[i];
            if (bg == null) { backgrounds.RemoveAt(i); continue; }
            bg.EvaluateEnvironment();
        }
    }

    void OnDestroy() { if (Instance == this) Instance = null; }
}