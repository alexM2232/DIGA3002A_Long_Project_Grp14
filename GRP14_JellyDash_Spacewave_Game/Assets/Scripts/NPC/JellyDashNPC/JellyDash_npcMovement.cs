using UnityEngine;


public class JD_NPCMovement : MonoBehaviour
{
    public enum MovementMode
    {
        SpaceWaves,
        JellyDash,
        JellyDashDown,
        JellyDashSwing
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
    
    [Header("Swinging")]
    public LayerMask swingMask;
    public float swingDetectRadius = 3f;
    public Transform swingCheck;
    public float swingForce = 6f;
    public float releaseOffset = 0.5f;
    public float maxSwingTime = 2.5f;
    public Vector2 releaseBoost = new Vector2(3f,3f);
    public float regrabCooldown = 0.5f;

    [Header("Rope Visual")]
    public LineRenderer ropeLine;
    public float ropeWidth = 0.08f;
    

    [Header("Downward Traversal")]
    public Transform edgeCheck;
    public float edgeCheckDistance = 0.4f;
    public LayerMask wallMask;
    public float wallcheckDistance = 0.3f;
    public float turnCooldown = 0.3f;

    private Rigidbody2D rb;
    private float nextDecisionTime;
    private int verticalDirection =-1;
    private float horizontalDirection = 1f;
    private bool isGrounded;

    private bool isTouchingWall;
    private int jumpDirection = -1;
    private bool wallJumping;
    private float wallJumpCooldown = 0.5f;
    private float nextWallJumpTime;
    private int moveDirection = 1;

    private float nextTurnTime;

    private DistanceJoint2D swingJoint;
    private Transform currentAnchor;
    private bool isSwinging;
    private float swingStartTime;
    private float nextGrabTime;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        SetupRope();
    }

    private void SetupRope()
    {
        if (ropeLine == null)
        {
            GameObject ropeObj = new GameObject("SwingRope");
            ropeObj.transform.SetParent(transform);
            ropeLine = ropeObj.AddComponent<LineRenderer>();
        }

        ropeLine.positionCount = 2;
        ropeLine.useWorldSpace = true;
        ropeLine.startWidth = ropeWidth;
        ropeLine.endWidth = ropeWidth;
        ropeLine.sortingOrder = 10;
        ropeLine.enabled = false;
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
            
            case MovementMode.JellyDashSwing:
                MoveSwing();
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

        float dirSign = Mathf.Sign(horizontalDirection);

        //Automatic forward movement
        rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, rb.linearVelocity.y);

        //Detect obstacles
        float castRadius = profile.radius + profile.safetyMargin;

        //Basic obtacle avoidance 
        RaycastHit2D obstacle = Physics2D.CircleCast(rb.position,castRadius, Vector2.right * dirSign,detectionDistance,profile.obstacleMask);

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
        if (isTouchingWall && !isGrounded && Time.time >= nextWallJumpTime)
        {
            horizontalDirection *= -1f;   // actually turn around, otherwise the next frame overrides it

             rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, 0f);
        rb.AddForce(new Vector2(horizontalDirection * wallJumpForce, jumpForce), ForceMode2D.Impulse);

        nextWallJumpTime = Time.time + wallJumpCooldown;
        isTouchingWall = false;

        // Keep sprite facing the new direction
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalDirection);
        transform.localScale = scale;
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
        else if (other.CompareTag("JellyDashSwingZone"))
        {
            SwitchMode(MovementMode.JellyDashSwing);
        }
    }

    public void SwitchMode(MovementMode newMode)
    {
        
        currentMode = newMode;

        if (isSwinging) ReleaseSwing();

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

    private bool IsPlatformAhead()
    {
        return Physics2D.Raycast(edgeCheck.position, Vector2.down, edgeCheckDistance, groundMask).collider != null;
    }

   private void MoveJellyDashDown()
{
    rb.gravityScale = 1f;

    //Look for wall
    float castRadius = profile.radius + profile.safetyMargin;
    Vector2 dir = Vector2.right * Mathf.Sign(horizontalDirection);

    RaycastHit2D wallHit = Physics2D.CircleCast(rb.position, castRadius, dir, wallcheckDistance, wallMask);

    //Turn around
    if(wallHit.collider != null && Time.time >= nextTurnTime)
    {
        horizontalDirection *= -1f;
        nextTurnTime = Time.time + turnCooldown;

        //Flip sprite
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalDirection);
        transform.localScale = scale;
    }

   //Keep walking
   rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, rb.linearVelocity.y); 

}

    private void MoveSwing()
{
    rb.gravityScale = 1f;

    if (!isSwinging)
    {
        // Run forward until an anchor comes into range
        rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, rb.linearVelocity.y);

        if (Time.time >= nextGrabTime)
            TryGrabAnchor();
        return;
    }

    // --- Currently swinging ---
    // Keep momentum going in the travel direction
    rb.AddForce(Vector2.right * horizontalDirection * swingForce);

    // Rope visual follows the NPC every physics step
    ropeLine.SetPosition(0, rb.position);
    ropeLine.SetPosition(1, currentAnchor.position);

    // Release once we've swung past the anchor, or after too long
    float pastAnchor = (rb.position.x - currentAnchor.position.x) * Mathf.Sign(horizontalDirection);
    if (pastAnchor >= releaseOffset || Time.time - swingStartTime >= maxSwingTime)
    {
        ReleaseSwing();
    }
}

private void TryGrabAnchor()
{
    Vector2 origin = swingCheck != null ? (Vector2)swingCheck.position : rb.position;

    Collider2D[] hits = Physics2D.OverlapCircleAll(origin, swingDetectRadius, swingMask);

    Collider2D best = null;
    float bestDist = float.MaxValue;

    foreach (Collider2D hit in hits)
    {
        // Only anchors ahead of us
        float ahead = (hit.transform.position.x - rb.position.x) * Mathf.Sign(horizontalDirection);
        if (ahead <= 0f) continue;

        float dist = Vector2.Distance(origin, hit.transform.position);
        if (dist < bestDist)
        {
            bestDist = dist;
            best = hit;
        }
    }

    if (best != null)
        StartSwing(best.transform);
}

private void StartSwing(Transform anchor)
{
    currentAnchor = anchor;
    isSwinging = true;
    swingStartTime = Time.time;

    swingJoint = gameObject.AddComponent<DistanceJoint2D>();
    swingJoint.autoConfigureConnectedAnchor = false;
    swingJoint.connectedAnchor = anchor.position;     // World space (no connected body)
    swingJoint.autoConfigureDistance = false;
    swingJoint.distance = Vector2.Distance(rb.position, anchor.position);
    swingJoint.maxDistanceOnly = true;                // Rope: can't stretch, can go slack
    swingJoint.enableCollision = true;

    ropeLine.enabled = true;
    ropeLine.SetPosition(0, rb.position);
    ropeLine.SetPosition(1, anchor.position);
}

private void ReleaseSwing()
{
    if (swingJoint != null) Destroy(swingJoint);

    isSwinging = false;
    currentAnchor = null;
    ropeLine.enabled = false;
    nextGrabTime = Time.time + regrabCooldown;

    // Fling forward off the rope
    rb.linearVelocity += new Vector2(releaseBoost.x * horizontalDirection, releaseBoost.y);
}
}
