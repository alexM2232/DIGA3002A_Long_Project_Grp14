

using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementJellyDash : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forwardPushSpeed = 5f;

    [Header("Normal Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;

    [Header("Tags")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string climbingWallTag = "ClimbingWall";
    [SerializeField] private string swingWallTag = "SwingWall";

    [Header("Wall Jump")]
    [SerializeField] private float wallJumpHorizontalForce = 8f;
    [SerializeField] private float wallJumpVerticalForce = 10f;

    [Header("Swing")]
    [SerializeField] private Transform swingVisual;
    [SerializeField] private float swingRopeLength = 2.5f;
    [SerializeField] private float swingGravity = 20f;
    [SerializeField] private float swingLaunchBoost = 3f;

    [Tooltip("Maximum distance the player can be from the swing point to attach.")]
    [SerializeField] private float maxSwingAttachDistance = 3f;

    private Rigidbody2D rb;

    private bool isGrounded;
    private bool touchingClimbingWall;
    private bool isSwinging;

    private float horizontalInput;
    private float normalGravityScale;

    private Transform swingWallTransform;
    private Vector2 swingAnchor;
    private float swingRadius;
    private float swingAngle;
    private float swingAngularVelocity;
    private float swingDirection = 1f;

    private Collider2D currentSwingCollider;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();

        if (rb == null)
        {
            Debug.LogError("TestPlayer needs a Rigidbody2D.");
            enabled = false;
            return;
        }

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        normalGravityScale = rb.gravityScale;

        if (swingVisual == null)
            swingVisual = transform.Find("SwingVisual");

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);
        else
            Debug.LogWarning("Assign SwingVisual in the Inspector.");
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;

        if (Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;

        // Release swing when Space is released.
        if (isSwinging)
        {
            if (!Keyboard.current.spaceKey.isPressed)
                ReleaseSwing();

            return;
        }

        // Climbing wall jump.
        if (touchingClimbingWall)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
                WallJump(-1f);

            else if (Keyboard.current.dKey.wasPressedThisFrame)
                WallJump(1f);
        }
        // Normal ground jump.
        else if (isGrounded &&
                 Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging)
        {
            HandleSwingMovement();
            return;
        }

        if (touchingClimbingWall)
            return;

        float horizontalMovement = horizontalInput == 0f
            ? forwardPushSpeed
            : horizontalInput * moveSpeed;

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            rb.linearVelocity.y
        );

        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    // WALL JUMP
    private void WallJump(float direction)
    {
        rb.linearVelocity = new Vector2(
            direction * wallJumpHorizontalForce,
            wallJumpVerticalForce
        );

        touchingClimbingWall = false;
        isGrounded = false;

        Debug.Log("Wall jump!");
    }

    // COLLISION DETECTION
    private void OnCollisionStay2D(Collision2D collision)
    {
        if (HasTagInParents(collision.transform, climbingWallTag))
            touchingClimbingWall = true;

        if (HasTagInParents(collision.transform, groundTag))
        {
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
        if (HasTagInParents(collision.transform, climbingWallTag))
            touchingClimbingWall = false;

        if (HasTagInParents(collision.transform, groundTag))
            isGrounded = false;
    }

    private bool HasTagInParents(Transform current, string targetTag)
    {
        while (current != null)
        {
            if (current.CompareTag(targetTag))
                return true;

            current = current.parent;
        }

        return false;
    }

    // SWING DETECTION
    private void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log("[PLAYER TEST] Entered trigger: " + other.gameObject.name);

            TryAttachToSwing(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        if (!isSwinging)
            TryAttachToSwing(other);
    }

    private void TryAttachToSwing(Collider2D other)
    {
        if (isSwinging)
            return;

        if (Keyboard.current == null ||
            !Keyboard.current.spaceKey.isPressed)
            return;

        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag(swingWallTag))
            {
                // Supports Polygon Collider 2D.
                Collider2D wallCollider = other;

                // Calculate the swing point.
                Bounds bounds = wallCollider.bounds;

                Vector2 potentialAnchor = new Vector2(
                    bounds.center.x,
                    bounds.max.y
                );

                // CHECK PLAYER DISTANCE BEFORE ATTACHING.
                float distanceToSwingPoint = Vector2.Distance(
                    rb.position,
                    potentialAnchor
                );

                if (distanceToSwingPoint > maxSwingAttachDistance)
                {
                    Debug.Log("Too far away from swing wall to attach.");
                    return;
                }

                AttachToSwing(current, wallCollider, potentialAnchor);
                return;
            }

            current = current.parent;
        }
    }

    // ATTACH TO SWING WALL
    private void AttachToSwing(
        Transform wall,
        Collider2D wallCollider,
        Vector2 anchor)
    {
        swingAnchor = anchor;

        swingWallTransform = wall;
        currentSwingCollider = wallCollider;

        Vector2 offset = rb.position - swingAnchor;

        if (offset.magnitude < 0.1f)
        {
            Debug.LogWarning("Player is too close to the swing pivot.");
            return;
        }

        // Keep the player's actual distance from the pivot.
        // This prevents snapping when attaching.
        swingRadius = offset.magnitude;

        swingAngle = Mathf.Atan2(
            offset.x,
            -offset.y
        );

        Vector2 tangent = new Vector2(
            Mathf.Cos(swingAngle),
            Mathf.Sin(swingAngle)
        );

        swingAngularVelocity = Vector2.Dot(
            rb.linearVelocity,
            tangent
        ) / swingRadius;

        swingDirection = Mathf.Sign(swingAngularVelocity);

        if (swingDirection == 0f)
            swingDirection = 1f;

        isSwinging = true;
        isGrounded = false;
        touchingClimbingWall = false;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(true);

        Debug.Log("Attached to swing wall: " + wall.name);
    }

    // SWING MOVEMENT
    private void HandleSwingMovement()
    {
        if (swingWallTransform == null ||
            currentSwingCollider == null)
        {
            ReleaseSwing();
            return;
        }

        swingAngularVelocity +=
            -(swingGravity / swingRadius) *
            Mathf.Sin(swingAngle) *
            Time.fixedDeltaTime;

        swingAngle += swingAngularVelocity *
            Time.fixedDeltaTime;

        Vector2 playerPosition = swingAnchor + new Vector2(
            Mathf.Sin(swingAngle) * swingRadius,
            -Mathf.Cos(swingAngle) * swingRadius
        );

        rb.MovePosition(playerPosition);

        Vector2 tangent = new Vector2(
            Mathf.Cos(swingAngle),
            Mathf.Sin(swingAngle)
        );

        rb.linearVelocity = tangent *
            swingAngularVelocity *
            swingRadius;

        UpdateSwingVisual(swingAnchor, playerPosition);
    }

    // UPDATE ROPE VISUAL
    private void UpdateSwingVisual(
        Vector2 anchorPosition,
        Vector2 playerPosition)
    {
        if (swingVisual == null)
            return;

        Vector2 direction = playerPosition - anchorPosition;

        swingVisual.position =
            (anchorPosition + playerPosition) / 2f;

        swingVisual.rotation = Quaternion.Euler(
            0f,
            0f,
            Mathf.Atan2(direction.y, direction.x) *
            Mathf.Rad2Deg - 90f
        );

        Vector3 scale = swingVisual.localScale;
        scale.y = direction.magnitude;
        swingVisual.localScale = scale;
    }

    // RELEASE SWING
    private void ReleaseSwing()
    {
        if (!isSwinging)
            return;

        Vector2 launchVelocity = rb.linearVelocity;

        launchVelocity.x += swingDirection * swingLaunchBoost;

        launchVelocity.y = Mathf.Max(
            launchVelocity.y,
            jumpForce * 0.5f
        );

        isSwinging = false;
        swingWallTransform = null;
        currentSwingCollider = null;

        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = launchVelocity;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);

        Debug.Log("Released swing!");
    }
}
