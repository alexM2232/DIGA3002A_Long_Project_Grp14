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
    private MovementMode startingMode =
        MovementMode.JellyDash;

    private PlayerMovementJellyDashTest jellyDash;
    private SWPlayerControllerTest spaceWaves;

    private MovementMode currentMode;

    private void Awake()
    {
        jellyDash =
            GetComponent<PlayerMovementJellyDashTest>();

        spaceWaves =
            GetComponent<SWPlayerControllerTest>();
    }

    private void Start()
    {
        SetMovementMode(startingMode);
    }

    public void SetMovementMode(
        MovementMode newMode)
    {
        currentMode = newMode;

        if (jellyDash != null)
        {
            jellyDash.enabled =
                newMode == MovementMode.JellyDash;
        }

        if (spaceWaves != null)
        {
            spaceWaves.enabled =
                newMode == MovementMode.SpaceWaves;
        }
    }

    public MovementMode GetMovementMode()
    {
        return currentMode;
    }
}