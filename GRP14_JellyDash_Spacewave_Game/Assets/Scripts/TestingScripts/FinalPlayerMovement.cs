
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
