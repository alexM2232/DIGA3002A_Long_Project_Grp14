
using UnityEngine;

public class FinalPlayerMovement : MonoBehaviour
{
    public enum MovementMode
    {
        JellyDash,
        SpaceWaves
    }

    [Header("Starting Movement Mode")]
    [SerializeField]
    private MovementMode startingMode = MovementMode.JellyDash;

    [Header("Player Visuals")]
    [SerializeField]
    private PlayerVisualController playerVisuals;

    private PlayerMovementJellyDash jellyDash;
    private SWPlayerControllerTest spaceWaves;

    private MovementMode currentMode;

    private void Awake()
    {
        jellyDash = GetComponent<PlayerMovementJellyDash>();
        spaceWaves = GetComponent<SWPlayerControllerTest>();

        if (jellyDash == null)
            Debug.LogError("PlayerMovementJellyDash not found!", this);

        if (spaceWaves == null)
            Debug.LogError("SWPlayerControllerTest not found!", this);

        if (playerVisuals == null)
            playerVisuals = GetComponent<PlayerVisualController>();
    }

    private void Start()
    {
        SetMovementMode(startingMode);
    }

    public void SetMovementMode(MovementMode newMode)
    {
        currentMode = newMode;

        if (jellyDash != null)
            jellyDash.enabled = newMode == MovementMode.JellyDash;

        if (spaceWaves != null)
            spaceWaves.enabled = newMode == MovementMode.SpaceWaves;

        // Switch player animations and trail.
        if (playerVisuals != null)
        {
            if (newMode == MovementMode.JellyDash)
                playerVisuals.EnterJellyDash();
            else
                playerVisuals.EnterSpaceWave();
        }

        // Switch music.
        if (AudioManager.Instance != null)
        {
            if (newMode == MovementMode.JellyDash)
                AudioManager.Instance.PlayJellyDashMusic();
            else
                AudioManager.Instance.PlaySpaceWavesMusic();
        }

        Debug.Log("Movement mode changed to: " + newMode);
    }

    public MovementMode GetMovementMode()
    {
        return currentMode;
    }
}
