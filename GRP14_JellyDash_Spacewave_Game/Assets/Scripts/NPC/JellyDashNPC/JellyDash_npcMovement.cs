using UnityEngine;


public class JD_NPCMovement : MonoBehaviour
{
    public enum MovementMode
    {
        SpaceWaves,
        JellyDash,
        JellyDashDown
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
    
    [Header("Downward Traversal")]
    public float horizontalJumpSpeed = 3f;
    public float downwardJumpForce = 5f;
    public float jumpCooldown = 0.25f;
    public float landingCheckDistance = 2f;


    private Rigidbody2D rb;
    private float nextDecisionTime;
    private int verticalDirection =-1;
    private float horizontalDirection = 1f;
    private bool isGrounded;
    private bool isTouchingWall;
    private int jumpDirection = -1;
    private bool isJumpingDown;
    private bool hasLeftGround;
    private float nextJumpTime;

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

            case MovementMode.JellyDashDown:
                MoveJellyDashDown();
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
        rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, rb.linearVelocity.y);

        //Detect obstacles
        float castRadius = profile.radius + profile.safetyMargin;

        //Basic obtacle avoidance 
        RaycastHit2D obstacle = Physics2D.CircleCast(rb.position,castRadius, Vector2.right,detectionDistance,profile.obstacleMask);

        if (obstacle.collider != null)
        {
            Debug.Log("OBSTACLE DETECTED: " + obstacle.collider.name);
            Debug.Log("IS GROUNDED: " + isGrounded);

            if (isGrounded)
            {
                Jump();
            }
            else
            {
                Debug.Log("Cannot jump: NPC is not grounded.");
            }
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

        Debug.Log("NPC jumps");
    }

    private void UpdateGrounded()
    {
       if (groundCheck == null) 
       {
            isGrounded = false;
            return;
       }
       

        isGrounded = Physics2D.OverlapCircle(groundCheck.position,groundCheckDistance,groundMask) != null;

        Debug.Log("is on ground");
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
        else if (other.CompareTag("JellyDashDownZone"))
        {
            SwitchMode(MovementMode.JellyDashDown);
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

   private void MoveJellyDashDown()
{
    rb.gravityScale = 1f;

    // Maintain horizontal movement
    rb.linearVelocity = new Vector2(
        horizontalJumpSpeed * jumpDirection,
        rb.linearVelocity.y
    );

    // Start the downward jump
    if (!isJumpingDown && isGrounded &&
        Time.time >= nextJumpTime)
    {
        isJumpingDown = true;
        hasLeftGround = false;

        rb.linearVelocity = new Vector2(
            horizontalJumpSpeed * jumpDirection,
            0f
        );

        rb.AddForce(
            Vector2.down * downwardJumpForce,
            ForceMode2D.Impulse
        );
    }

    // Confirm that the NPC has actually left the platform
    if (isJumpingDown && !isGrounded)
    {
        hasLeftGround = true;
    }

    // Detect landing
    if (isJumpingDown && hasLeftGround && isGrounded)
    {
        isJumpingDown = false;

        // Reverse direction AFTER landing
        jumpDirection *= -1;

        nextJumpTime = Time.time + jumpCooldown;
    }
}
}
