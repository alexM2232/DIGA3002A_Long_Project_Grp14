using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Central authority for which environment the player is currently in.
///
/// Backgrounds register themselves in Start() (after all Awake()s have run,
/// so the singleton is guaranteed to exist). Whenever SetEnvironment() is
/// called, the manager pokes every registered background directly via
/// EvaluateEnvironment() — this works even for GameObjects hidden with
/// SetActive(false), because direct C# method calls on disabled components
/// still execute.
/// </summary>
public class EnvironmentManager : MonoBehaviour
{
    public static EnvironmentManager Instance { get; private set; }

    [Header("Startup")]
    [Tooltip("Environment that is active (and scrolling) from frame one. " +
             "Every OTHER environment stays frozen until its EnvironmentZone fires. " +
             "Leave empty to require a trigger even for the first environment.")]
    public string startingEnvironment = "Environment1";

    /// <summary>Id of the environment the player is currently inside.</summary>
    public string ActiveEnvironmentId { get; private set; }

    /// <summary>The environment the player was in before the current one (or null).</summary>
    public string PreviousEnvironmentId { get; private set; }

    /// <summary>Fired with the new environment id whenever it changes.</summary>
    public event Action<string> OnEnvironmentChanged;

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
        PreviousEnvironmentId = null;
    }

    // ---------------------------------------------------------------
    /// <summary>Called by EndlessBackground.Start(). Safe to call multiple times.</summary>
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

    // ---------------------------------------------------------------
    /// <summary>Switch the active environment. Call from EnvironmentZone triggers.</summary>
    public void SetEnvironment(string environmentId)
    {
        if (string.IsNullOrEmpty(environmentId)) return;
        if (environmentId == ActiveEnvironmentId) return;

        PreviousEnvironmentId = ActiveEnvironmentId;
        ActiveEnvironmentId = environmentId;
        Debug.Log($"[EnvironmentManager] '{PreviousEnvironmentId}' → '{ActiveEnvironmentId}'");

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

    // ---------------------------------------------------------------
    /// <summary>Revive every passed background back to dormant. Use on level restart.</summary>
    public void ResetAllPassed()
    {
        for (int i = backgrounds.Count - 1; i >= 0; i--)
        {
            if (backgrounds[i] == null) { backgrounds.RemoveAt(i); continue; }
            backgrounds[i].ResetPassed();
        }
        PreviousEnvironmentId = null;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }
}