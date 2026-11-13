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

    [Header("Swinging Obstacle")]
    public LayerMask swingHazardMask;          // Layer of the swinging blade/obstacle
    public float swingDangerDistance = 3f;     // How far ahead to check (cover the swing's sweep)
    public float swingZoneHeight = 3f;         // Height of the check area
    public float retreatDuration = 1.2f;       // How long it backs away before trying again
    public float retreatSpeedMultiplier = 1f;

    [Header("Wall Jump")]
    public LayerMask wallJumpMask;
    public float wallJumpCheckDistance = 0.1f;

    [Header("Hazard Arc Prediction")]
    public int arcSamples = 12;               // More samples = more accurate
    public float arcMaxDistance = 15f;        // Max shaft width to look across
    public float slideSpeed = 1.5f;   // How slowly it slides while waiting
    public LayerMask hazardMask;               // Deadly obstacles (keep OUT of wallJumpMask)
    public float hazardSideMargin = 0.25f;     // Extra width so near-misses count as hits

    
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

    [Header("Turn Around Layer")]
    public LayerMask turnMask;                 // Layer of the turn-around markers
    public float turnCheckDistance = 0.6f;     // How far ahead to look
    public float turnCooldownJD = 0.5f;          // Stops it flipping back and forth

    [Header("NPC Respawning")]
    public string obstacleTag = "Damage";
    public Transform[] checkPoints;

    [Header("Catch-Up Teleport")]
    public Transform player;
    public Transform[] checkpoints;              // Drag all checkpoint transforms here
    public float maxDistanceFromPlayer = 25f;
    public float teleportCooldown = 3f;          // Stops repeated teleports

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

    private bool isRetreating;
    private float retreatEndTime;
    private float approachDirection;    

    private float nextTurnTimeJD;

    private float mistakeLockUntil;

    private int lastCheckpointIndex = 0;   // Index of the last checkpoint reached

    private float nextTeleportTime;

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

        CheckTeleport();
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
            verticalDirection = Decide();

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

        HandleTurnMarker();
        if (HandleSwingObstacle()) return;

        if (HandleSwingObstacle()) return;

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

            if (isGrounded  && JumpDelayPassed())
            {
                Jump();
            }
            else
            {
                Debug.Log("Cannot jump: NPC is not grounded.");
            }
        }

        if (Time.time >= nextWallJumpTime && !isGrounded && IsWallAhead(dirSign))
{
    // After bouncing, the NPC travels the opposite way
    if (IsArcSafe(-dirSign, wallJumpForce) || TakesRisk())
    {
        WallJump();
    }
    else
    {
        // Unsafe: cling to the wall and slide down until the arc is clear
        rb.linearVelocity = new Vector2(0f, Mathf.Max(rb.linearVelocity.y, -slideSpeed));
    }
}
    }

  private bool IsWallAhead(float dirSign, float distance = -1f)
    {
        if (distance < 0f) distance = wallJumpCheckDistance;

        return Physics2D.CircleCast(rb.position,profile.radius,Vector2.right * dirSign,distance,wallJumpMask).collider != null;
    }

    // Simulates a wall jump launched toward travelDir with upward speed launchVy
private bool IsArcSafe(float travelDir, float launchVy)
{
    // Find the far wall so we know where the arc ends
    RaycastHit2D farWall = Physics2D.Raycast(
        rb.position, Vector2.right * travelDir, arcMaxDistance, wallJumpMask);

    float dist = farWall.collider != null ? farWall.distance : arcMaxDistance;

    float vx = profile.speed * travelDir;
    float g = Mathf.Abs(Physics2D.gravity.y * rb.gravityScale);
    float totalTime = dist / Mathf.Abs(vx);
    float checkRadius = profile.radius + hazardSideMargin;

    for (int i = 1; i <= arcSamples; i++)
    {
        float t = totalTime * i / arcSamples;

        Vector2 p = rb.position + new Vector2(
            vx * t,
            launchVy * t - 0.5f * g * t * t);

        if (Physics2D.OverlapCircle(p, checkRadius, hazardMask) != null)
            return false;   // The arc hits a hazard
    }
    return true;
}

    private void WallJump()
    {
        // Turn away from the wall
        horizontalDirection *= -1f;

        // Set velocity directly: away from the wall and upward
        rb.linearVelocity = new Vector2(profile.speed * horizontalDirection, wallJumpForce);

        nextWallJumpTime = Time.time + wallJumpCooldown;

        // Flip sprite
        Vector3 scale = transform.localScale;
        scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalDirection);
        transform.localScale = scale;

        Debug.Log("NPC wall jumps");
    }  

    private void Jump()
    {
        if (!IsArcSafe(Mathf.Sign(horizontalDirection), jumpForce) && !TakesRisk()) return;

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

         // (keep your existing zone checks too)
    if (other.CompareTag("Checkpoint"))
    {
        for (int i = 0; i < checkPoints.Length; i++)
        {
            if (checkPoints[i] == other.transform && i > lastCheckpointIndex)
            {
                lastCheckpointIndex = i;
                break;
            }
        }
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

// Returns true if this method controlled movement this frame
private bool HandleSwingObstacle()
{
    if (!isGrounded) return false;

    // Currently backing away
    if (isRetreating)
    {
        if (Time.time < retreatEndTime)
        {
            float retreatDir = -approachDirection;
            float retreatSign = Mathf.Sign(retreatDir);

            // Stop if there's a wall or turn marker behind us; just wait
            float vx = (IsWallAhead(retreatSign) || IsTurnMarkerAhead(retreatSign))
                ? 0f
                : profile.speed * retreatSpeedMultiplier * retreatDir;

            rb.linearVelocity = new Vector2(vx, rb.linearVelocity.y);
            return true;
        }

        // Retreat finished: go back to normal movement toward the obstacle
        isRetreating = false;
    }

    // Path blocked by the swing? Start a retreat
    if (IsSwingZoneBlocked() && !TakesRisk(0.5f))
    {
        isRetreating = true;
        approachDirection = horizontalDirection;
        retreatEndTime = Time.time + retreatDuration;
        return true;
    }

    return false;
}

private bool IsSwingZoneBlocked()
{
    float dirSign = Mathf.Sign(horizontalDirection);

    Vector2 center = rb.position + new Vector2(dirSign * swingDangerDistance * 0.5f, 0f);
    Vector2 size = new Vector2(swingDangerDistance, swingZoneHeight);

    return Physics2D.OverlapBox(center, size, 0f, swingHazardMask) != null;
}

private bool IsTurnMarkerAhead(float dirSign)
{
    return Physics2D.CircleCast(
        rb.position,
        profile.radius,
        Vector2.right * dirSign,
        turnCheckDistance,
        turnMask).collider != null;
}

// Returns true if the NPC turned this frame
private bool HandleTurnMarker()
{
    if (Time.time < nextTurnTimeJD) return false;

    float dirSign = Mathf.Sign(horizontalDirection);
    if (!IsTurnMarkerAhead(dirSign)) return false;

    horizontalDirection *= -1f;
    nextTurnTimeJD = Time.time + turnCooldownJD;
    FaceDirection();   // you already have this from the earlier code
    return true;
}

private void FaceDirection()
{
    Vector3 scale = transform.localScale;
    scale.x = Mathf.Abs(scale.x) * Mathf.Sign(horizontalDirection);
    transform.localScale = scale;
}

 private void OnCollisionEnter2D(Collision2D collision)
{
    if (collision.gameObject.CompareTag(obstacleTag))
    {
        RespawnAtNearestCheckpoint();
    }
}


private void RespawnAtNearestCheckpoint()
{
    if (checkPoints == null || checkPoints.Length == 0)
    {
        Debug.LogWarning("NPC respawn failed: No checkpoints assigned!");
        return;
    }

    Transform cp = checkPoints[Mathf.Clamp(lastCheckpointIndex, 0, checkPoints.Length - 1)];
    if (cp == null) return;

    if (isSwinging) ReleaseSwing();

    rb.linearVelocity = Vector2.zero;
    rb.position = cp.position;

    Debug.Log("NPC respawned at: " + cp.name);
}

private int Decide()
{
    // Committed to a previous mistake: keep going the same way
    if (Time.time < mistakeLockUntil)
        return verticalDirection;

    float reck = profile.recklessness;

    // Reckless NPCs leave less room and look less far ahead
    float margin = profile.safetyMargin * (1f - reck);
    float castRadius = profile.radius + margin;
    float distance = profile.speed * profile.stepTime * Mathf.Lerp(1f, 0.5f, reck);
    Vector2 pos = rb.position;

    bool upBlocked = Physics2D.CircleCast(pos, castRadius, new Vector2(1f, 1f).normalized,
                         distance, profile.obstacleMask).collider != null;
    bool downBlocked = Physics2D.CircleCast(pos, castRadius, new Vector2(1f, -1f).normalized,
                         distance, profile.obstacleMask).collider != null;

    // What a perfect NPC would do
    int best = verticalDirection;
    if (upBlocked && !downBlocked) best = -1;
    else if (!upBlocked && downBlocked) best = 1;

    // Reckless NPCs make more mistakes
    float chance = Mathf.Clamp01(profile.mistakeChance + reck * 0.3f);

    // Needs to change course but may fail to react
    if (best != verticalDirection)
    {
        if (Random.value < chance)
        {
            mistakeLockUntil = Time.time + profile.mistakeHoldTime;
            return verticalDirection;
        }
        return best;
    }

    // Path is clear: reckless NPCs wobble more
    if (!upBlocked && !downBlocked && Random.value < profile.wobbleChance * (1f + reck * 3f))
    {
        mistakeLockUntil = Time.time + profile.mistakeHoldTime;
        return -verticalDirection;
    }

    return best;
}

// True if this NPC decides to ignore a danger this time
private bool TakesRisk(float scale = 1f)
{
    return Random.value < profile.recklessness * profile.riskyJumpChance * scale;
}

private float nextJumpAllowedTime;
private bool jumpDelayArmed;

// Reckless NPCs react late; cautious ones react immediately
private bool JumpDelayPassed()
{
    if (!jumpDelayArmed)
    {
        float delay = profile.recklessness * Random.Range(0f, profile.reactionDelayMax);
        nextJumpAllowedTime = Time.time + delay;
        jumpDelayArmed = true;
    }

    if (Time.time < nextJumpAllowedTime) return false;

    jumpDelayArmed = false;   // reset for the next obstacle
    return true;
}

private void CheckTeleport()
{
    if (player == null || checkpoints == null || checkpoints.Length == 0) return;
    if (Time.time < nextTeleportTime) return;
    if (Vector2.Distance(rb.position, player.position) <= maxDistanceFromPlayer) return;

    // Find the checkpoint closest to the PLAYER
    Transform nearest = null;
    float best = float.MaxValue;

    foreach (Transform cp in checkpoints)
    {
        if (cp == null) continue;
        float d = Vector2.Distance(player.position, cp.position);
        if (d < best)
        {
            best = d;
            nearest = cp;
        }
    }

    if (nearest != null) TeleportTo(nearest);
}

private void TeleportTo(Transform cp)
{
    // Let go of any swing first
    if (isSwinging) ReleaseSwing();

    rb.position = cp.position;
    rb.linearVelocity = Vector2.zero;

    // Reset temporary behaviour state
    isRetreating = false;
    jumpDelayArmed = false;
    mistakeLockUntil = 0f;
    nextTurnTime = 0f;

    // Optional: take the movement mode and direction stored on the checkpoint
    NPCCheckpoint data = cp.GetComponent<NPCCheckpoint>();
    if (data != null)
    {
        SwitchMode(data.mode);
        horizontalDirection = data.direction;
        FaceDirection();
    }

    nextTeleportTime = Time.time + teleportCooldown;
}
}
