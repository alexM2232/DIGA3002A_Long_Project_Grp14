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

    
    private PlayerMovementJellyDash jellyDash;
    private SWPlayerControllerTest spaceWaves;

  

    private MovementMode currentMode;

    private void Awake()
    {
        jellyDash =
            GetComponent<PlayerMovementJellyDash>();

        spaceWaves =
            GetComponent<SWPlayerControllerTest>();

        if (jellyDash == null)
        Debug.LogError("FinalPlayerMovement: JellyDash script not found on " + name, this);

    if (spaceWaves == null)
        Debug.LogError("FinalPlayerMovement: SpaceWaves script not found on " + name, this);
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

        Debug.Log($"Mode set: {newMode} | jelly enabled: {jellyDash?.enabled} | waves enabled: {spaceWaves?.enabled}");


      
    }

    public MovementMode GetMovementMode()
    {
        return currentMode;
    }
}