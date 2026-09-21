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

        // Makes the player movement smoother.
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
        // Automatic movement forward.
        float horizontalMovement = forwardDirection * moveSpeed;

        // A/D changes the direction.
        if (horizontalInput != 0f)
        {
            horizontalMovement = horizontalInput * moveSpeed;
        }

        // IMPORTANT:
        // Keep the player's current Y velocity so gravity and jumping work.
        float verticalMovement = rb.linearVelocity.y;

        // W can move the player upward.
        if (verticalInput > 0f)
        {
            verticalMovement = verticalSpeed;
        }

        // S can move the player downward.
        if (verticalInput < 0f)
        {
            verticalMovement = -verticalSpeed;
        }

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            verticalMovement
        );
    }

    void CheckJump()
    {
        // SPACE makes the player jump.
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
        // Makes the player fall faster after jumping.
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
            foreach (ContactPoint2D contact in collision.contacts)
            {
                // Player is standing on top of the ground.
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