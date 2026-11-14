using UnityEngine;
using UnityEngine.InputSystem;

public class SWPlayerController : MonoBehaviour
{
    public float speed = 5f;
    public float waveSpeed = 5f;
    private Rigidbody2D rb;
    private bool waveInput;
    
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        if (waveInput){
            rb.linearVelocity = new Vector2(speed, waveSpeed); //Go up
        }
        else{
            rb.linearVelocity = new Vector2(speed, -waveSpeed); //Go down
        }
    }

    public void onWave(InputAction.CallbackContext context){
        waveInput = context.ReadValueAsButton();
    }
}
