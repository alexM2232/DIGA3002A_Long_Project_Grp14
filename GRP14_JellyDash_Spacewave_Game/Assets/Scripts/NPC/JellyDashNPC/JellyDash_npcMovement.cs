using UnityEngine;


public class JD_NPCMovement : MonoBehaviour
{
    public enum MovementMode
    {
        SpaceWaves,
        JellyDash
    }

    [Header("Configuration")]
    public npcProfile profile;
    public MovementMode currentMode = MovementMode.JellyDash;

    [Header("Jelly Dash")]
    public float jumpForce = 8f;
    public float wallJumpForce = 7f;
    public float groundCheckDistance = 0.15f;
    public Transform groundCheck;
    public LayerMask groundMask;

    [Header("Obstacle Detection")]
    public float detectionDistance = 1.5f;

    private Rigidbody2D rb;
    private float nextDecisionTime;
    private int verticalDirection =-1;
    private bool isGrounded;
    private bool isTouchingWall;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    private void FixedUpdate()
    {
        if (profile == null) return;

        UpdateGrounded();

        switch (currentMode)
        {
            case MovementMode.SpaceWaves:
                MoveSpaceWaves();
                break;

            case MovementMode.JellyDash:
                MoveJellyDash();
                break;
        }
    }

    private void MoveSpaceWaves()
    {
        rb.gravityScale = 0f;

        if (Time.time >= nextDecisionTime)
        {
            verticalDirection = ChooseVerticalDirection();

            nextDecisionTime = Time.time + profile.decisionInterval;
        }

        rb.linearVelocity = new Vector2(profile.speed, verticalDirection * profile.speed);
    }

    private int ChooseVerticalDirection() //Checks if routes are clear
    {
        float castRadius = profile.radius + profile.safetyMargin;

        Vector2 position = rb.position;
        float distance = profile.speed * profile.stepTime;

        bool upBlocked = Physics2D.CircleCast(position,castRadius,new Vector2(1f,1f).normalized,distance,profile.obstacleMask).collider != null;
        bool downBlocked = Physics2D.CircleCast(position,castRadius,new Vector2(1f,-1f).normalized,distance,profile.obstacleMask).collider != null;

        if (upBlocked && !downBlocked) return -1;
        if (!upBlocked && downBlocked) return 1;

        return verticalDirection;
    }

    private void MoveJellyDash()
    {
        rb.gravityScale = 1f;

        //Automatic forward movement
        rb.linearVelocity = new Vector2(profile.speed, rb.linearVelocity.y);

        //Basic obtacle avoidance 
        RaycastHit2D obstacle = Physics2D.Raycast(rb.position,Vector2.right,detectionDistance,profile.obstacleMask);

        if (obstacle.collider != null && isGrounded)
        {
            Jump();
        }

        //Wall jump
        if (isTouchingWall && !isGrounded)
        {
            rb.linearVelocity = new Vector2(-profile.speed,rb.linearVelocity.y);

            rb.AddForce(new Vector2(-Mathf.Sign(transform.localScale.x) * wallJumpForce, jumpForce), ForceMode2D.Impulse);
        }
    }

    private void Jump()
    {
        rb.linearVelocity = new Vector2(rb.linearVelocity.x, 0f);

        rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse);
    }

    private void UpdateGrounded()
    {
        isGrounded = false;

        if (groundCheck == null) return;

        isGrounded = Physics2D.Raycast(groundCheck.position,Vector2.down,groundCheckDistance,groundMask).collider != null;
    }

     private void OnCollisionStay2D(Collision2D collision)
    {
        if (((1 << collision.gameObject.layer) &
             groundMask.value) != 0)
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    isTouchingWall = true;
                    return;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        isTouchingWall = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("SpaceWavesZone"))
        {
            SwitchMode(MovementMode.SpaceWaves);
        }
        else if (other.CompareTag("JellyDashZone"))
        {
            SwitchMode(MovementMode.JellyDash);
        }
    }

    public void SwitchMode(MovementMode newMode)
    {
        currentMode = newMode;

        if (newMode == MovementMode.SpaceWaves)
        {
            rb.gravityScale = 0f;
        }
        else
        {
            rb.gravityScale = 1f;
        }

        nextDecisionTime = 0f;
    }
}
