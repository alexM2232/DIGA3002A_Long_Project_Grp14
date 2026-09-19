using UnityEngine;

public class PlayerMovementJellyDash : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    [Header("Gravity")]
    public float fallGravityMultiplier = 2.5f;

    private Rigidbody2D rb;

    private bool isGrounded;

    private float horizontalInput;
    private float verticalInput;

    private float forwardDirection = 1f;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        // Makes movement smoother.
        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
    }

    void Update()
    {
        GetMovementInput();
        CheckJump();
    }

    void FixedUpdate()
    {
        MovePlayer();
        ApplyBetterGravity();
    }

    void GetMovementInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        // A = Move Left
        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
            forwardDirection = -1f;
        }

        // D = Move Right
        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
            forwardDirection = 1f;
        }

        // W = Move Up
        // W does NOT jump.
        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }

        // S = Move Down
        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }
    }

    void MovePlayer()
    {
        // Automatic forward movement.
        float horizontalMovement = forwardDirection * moveSpeed;

        // A/D can change the direction.
        if (horizontalInput != 0f)
        {
            horizontalMovement = horizontalInput * moveSpeed;
        }

        // W/S controls vertical movement.
        float verticalMovement = verticalInput * verticalSpeed;

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            verticalMovement
        );
    }

    void CheckJump()
    {
        // SPACE is the ONLY jump button.
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;

            Debug.Log("JUMP!");
        }
    }

    void ApplyBetterGravity()
    {
        // When the player is falling,
        // increase gravity to make the fall feel stronger.
        if (rb.linearVelocity.y < 0)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1) *
                Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            // Check that the player is actually landing
            // on top of the ground.
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.5f)
                {
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}

