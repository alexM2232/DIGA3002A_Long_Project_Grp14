
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    public float speed = 5f;
    public float jumpForce = 8f;

    private Rigidbody2D rb;

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    void Update()
    {
        // Horizontal movement
        float moveInput = 0f;
        if (Input.GetKey(KeyCode.D)) moveInput = 1f;
        if (Input.GetKey(KeyCode.A)) moveInput = -1f;

        rb.linearVelocity = new Vector2(moveInput * speed, rb.linearVelocity.y);

        // Jump (works anytime, even mid-air)
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }
}