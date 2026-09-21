using UnityEngine;

public class PlayerMovementJellyDashTest : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float verticalSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 2.5f;

    private Rigidbody2D rb;

    private bool isGrounded;

    private float horizontalInput;
    private float verticalInput;

    private float forwardDirection = 1f;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError(
                "PlayerMovementJellyDashTest: " +
                "Rigidbody2D is missing from the Player."
            );

            return;
        }

        rb.interpolation =
            RigidbodyInterpolation2D.Interpolate;
    }

    private void Update()
    {
        if (rb == null)
            return;

        GetMovementInput();
        CheckJump();
    }

    private void FixedUpdate()
    {
        if (rb == null)
            return;

        MovePlayer();
        ApplyBetterGravity();
    }

    private void GetMovementInput()
    {
        horizontalInput = 0f;
        verticalInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
            forwardDirection = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
            forwardDirection = 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }
    }

    private void MovePlayer()
    {
        float horizontalMovement =
            forwardDirection * moveSpeed;

        if (horizontalInput != 0f)
        {
            horizontalMovement =
                horizontalInput * moveSpeed;
        }

        float verticalMovement =
            rb.linearVelocity.y;

        if (verticalInput > 0f)
        {
            verticalMovement = verticalSpeed;
        }

        if (verticalInput < 0f)
        {
            verticalMovement = -verticalSpeed;
        }

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            verticalMovement
        );
    }

    private void CheckJump()
    {
        if (Input.GetKeyDown(KeyCode.Space) &&
            isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
        }
    }

    private void ApplyBetterGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity +=
                Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (!collision.gameObject.CompareTag("Ground"))
            return;

        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (contact.normal.y > 0.5f)
            {
                isGrounded = true;
                break;
            }
        }
    }

    private void OnCollisionExit2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = false;
        }
    }
}
