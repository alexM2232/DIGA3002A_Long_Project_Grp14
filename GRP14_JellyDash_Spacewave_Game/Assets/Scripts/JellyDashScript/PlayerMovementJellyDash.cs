using UnityEngine;

public class PlayerMovementJellyDash : MonoBehaviour
{
    [Header("Movement")]
    public float moveSpeed = 5f;
    public float verticalSpeed = 5f;

    [Header("Jump")]
    public float jumpForce = 10f;

    private Rigidbody2D rb;
    private bool isGrounded;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        MovePlayer();
        Jump();
    }

    void MovePlayer()
    {
        // W = Move Up
        // S = Move Down
        // A = Move Left
        // D = Move Right

        float horizontalInput = 0f;
        float verticalInput = 0f;

        if (Input.GetKey(KeyCode.A))
        {
            horizontalInput = -1f;
        }

        if (Input.GetKey(KeyCode.D))
        {
            horizontalInput = 1f;
        }

        if (Input.GetKey(KeyCode.W))
        {
            verticalInput = 1f;
        }

        if (Input.GetKey(KeyCode.S))
        {
            verticalInput = -1f;
        }

        // Automatic movement to the right
        float horizontalMovement = moveSpeed + (horizontalInput * moveSpeed);

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            verticalInput * verticalSpeed
        );
    }

    void Jump()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
        }
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Ground"))
        {
            isGrounded = true;
        }
    }
}
