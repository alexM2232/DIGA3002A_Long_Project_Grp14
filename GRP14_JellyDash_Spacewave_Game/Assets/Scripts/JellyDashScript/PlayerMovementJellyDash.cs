
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
    [SerializeField] private float swingLaunchAngle = 0.15f;

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

        if (isSwinging)
            return;

        if (touchingClimbingWall)
        {
            if (Keyboard.current.aKey.wasPressedThisFrame)
                WallJump(-1f);
            else if (Keyboard.current.dKey.wasPressedThisFrame)
                WallJump(1f);
        }
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

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag(climbingWallTag))
            touchingClimbingWall = true;

        if (collision.gameObject.CompareTag(groundTag))
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
        if (collision.gameObject.CompareTag(climbingWallTag))
            touchingClimbingWall = false;

        if (collision.gameObject.CompareTag(groundTag))
            isGrounded = false;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
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

        Transform current = other.transform;

        while (current != null)
        {
            if (current.CompareTag(swingWallTag))
            {
                BoxCollider2D wallCollider =
                    current.GetComponent<BoxCollider2D>();

                if (wallCollider == null)
                    wallCollider =
                        current.GetComponentInChildren<BoxCollider2D>();

                if (wallCollider == null)
                {
                    Debug.LogWarning(
                        current.name +
                        " needs a BoxCollider2D to calculate its pivot."
                    );
                    return;
                }

                AttachToSwing(current, wallCollider);
                return;
            }

            current = current.parent;
        }
    }

    private void AttachToSwing(
        Transform wall,
        BoxCollider2D wallCollider)
    {
        Bounds bounds = wallCollider.bounds;

        swingAnchor = new Vector2(
            bounds.center.x,
            bounds.max.y
        );

        swingWallTransform = wall;

        Vector2 offset = rb.position - swingAnchor;

        if (offset.magnitude < 0.1f)
        {
            Debug.LogWarning(
                "Player is too close to the swing pivot."
            );
            return;
        }

        swingRadius = swingRopeLength;
        swingAngle = Mathf.Atan2(offset.x, -offset.y);

        Vector2 tangent = new Vector2(
            Mathf.Cos(swingAngle),
            Mathf.Sin(swingAngle)
        );

        swingAngularVelocity = Vector2.Dot(
            rb.linearVelocity,
            tangent
        ) / offset.magnitude;

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

    private void HandleSwingMovement()
    {
        if (swingWallTransform == null)
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

        // Automatically launch after swinging upward
        // on the side the player is travelling toward.
        bool passedBottom =
            swingDirection > 0f
                ? swingAngle > swingLaunchAngle
                : swingAngle < -swingLaunchAngle;

        bool movingUpward =
            Mathf.Sign(swingAngularVelocity) == swingDirection;

        if (passedBottom && movingUpward)
            ReleaseSwing();
    }

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

        rb.gravityScale = normalGravityScale;
        rb.linearVelocity = launchVelocity;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);

        Debug.Log("Automatically launched from swing!");
    }
}
