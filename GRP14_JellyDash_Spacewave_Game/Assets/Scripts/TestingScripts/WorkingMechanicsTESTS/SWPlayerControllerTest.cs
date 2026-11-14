using UnityEngine;
using UnityEngine.InputSystem;

public class SWPlayerControllerTest : MonoBehaviour
{
    [Header("Space Waves Movement")]
    [SerializeField] private float speed = 5f;
    [SerializeField] private float waveSpeed = 5f;

    private Rigidbody2D rb;
    private bool waveInput;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "SWPlayerControllerTest: " +
                "Rigidbody2D is missing from the Player."
            );
        }
    }

    private void Update()
    {
        if (rb == null)
            return;

        if (waveInput)
        {
            rb.linearVelocity =
                new Vector2(speed, waveSpeed);
        }
        else
        {
            rb.linearVelocity =
                new Vector2(speed, -waveSpeed);
        }
    }

    public void OnWave(
        InputAction.CallbackContext context)
    {
        waveInput =
            context.ReadValueAsButton();
    }

    private void OnDisable()
    {
        waveInput = false;
    }
}