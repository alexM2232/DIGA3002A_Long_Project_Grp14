
using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(BoxCollider2D))]
[RequireComponent(typeof(SpriteRenderer))]
public class PlayerMovementJellyDash : MonoBehaviour
{
    [Header("Normal Movement")]
    [SerializeField] private float automaticMoveSpeed = 5f;
    [SerializeField] private float manualControlSpeed = 5f;

    [Header("Normal Jump")]
    [SerializeField] private float jumpForce = 10f;
    [SerializeField] private float fallGravityMultiplier = 1.5f;

    [Header("Direct Wall Climbing")]
    [SerializeField] private float climbingHorizontalSpeed = 5f;
    [SerializeField] private float climbingVerticalSpeed = 6f;
    [SerializeField] private float wallSlideSpeed = 5f;

    [Header("Sprite")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Header("Tags")]
    [SerializeField] private string groundTag = "Ground";
    [SerializeField] private string climbingWallTag = "ClimbingWall";
    [SerializeField] private string swingWallTag = "SwingWall";

    [Header("Swing")]
    [SerializeField] private Transform swingVisual;
    [SerializeField] private float swingGravity = 20f;
    [SerializeField] private float swingLaunchBoost = 1.5f;
    [SerializeField] private float swingDamping = 0.15f;
    [SerializeField] private float swingLaunchControlDuration = 0.35f;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;
    private SpriteRenderer swingVisualRenderer;

    private Vector2 originalColliderOffset;
    private float originalGravityScale;

    private float automaticDirection = 1f;
    private float horizontalInput;

    private bool isGrounded;
    private bool touchingClimbingWall;
    private bool isSwinging;

    private float swingLaunchControlTimer;

    private readonly HashSet<Collider2D> climbingWallContacts =
        new HashSet<Collider2D>();

    private readonly HashSet<Collider2D> groundContacts =
        new HashSet<Collider2D>();

    private Vector2 swingAnchorPosition;
    private float swingRadius;
    private float swingAngle;
    private float swingAngularVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (swingVisual != null)
        {
            swingVisualRenderer =
                swingVisual.GetComponent<SpriteRenderer>();

            if (swingVisualRenderer == null)
                swingVisualRenderer =
                    swingVisual.GetComponentInChildren<SpriteRenderer>();

            swingVisual.gameObject.SetActive(false);
        }

        originalColliderOffset = playerCollider.offset;
        originalGravityScale = rb.gravityScale;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        FaceDirection(1f);
    }

    private void Update()
    {
        if (Keyboard.current == null)
            return;

        horizontalInput = 0f;

        if (Keyboard.current.aKey.isPressed)
            horizontalInput = -1f;
        else if (Keyboard.current.dKey.isPressed)
            horizontalInput = 1f;

        if (horizontalInput != 0f && !isSwinging)
            FaceDirection(horizontalInput);

        if (isSwinging)
        {
            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
                ReleaseSwing();

            return;
        }

        if (isGrounded &&
            !touchingClimbingWall &&
            Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            rb.linearVelocity = new Vector2(
                rb.linearVelocity.x,
                jumpForce
            );

            isGrounded = false;
            groundContacts.Clear();
        }
    }

    private void FixedUpdate()
    {
        if (isSwinging)
        {
            HandleSwingMovement();
            return;
        }

        if (swingLaunchControlTimer > 0f)
        {
            swingLaunchControlTimer -= Time.fixedDeltaTime;
            ApplyFallGravity();
            return;
        }

        if (touchingClimbingWall)
        {
            if (horizontalInput != 0f)
            {
                rb.gravityScale = 0f;

                rb.linearVelocity = new Vector2(
                    horizontalInput * climbingHorizontalSpeed,
                    climbingVerticalSpeed
                );

                FaceDirection(horizontalInput);
            }
            else
            {
                rb.gravityScale = originalGravityScale;

                float verticalSpeed = rb.linearVelocity.y;

                if (verticalSpeed < 0f)
                    verticalSpeed = Mathf.Max(verticalSpeed, -wallSlideSpeed);

                rb.linearVelocity = new Vector2(0f, verticalSpeed);
            }

            return;
        }

        rb.gravityScale = originalGravityScale;

        if (horizontalInput != 0f)
        {
            rb.linearVelocity = new Vector2(
                horizontalInput * manualControlSpeed,
                rb.linearVelocity.y
            );
        }
        else
        {
            rb.linearVelocity = new Vector2(
                automaticDirection * automaticMoveSpeed,
                rb.linearVelocity.y
            );
        }

        ApplyFallGravity();
    }

    private void ApplyFallGravity()
    {
        if (rb.linearVelocity.y < 0f)
            rb.gravityScale = originalGravityScale * fallGravityMultiplier;
        else
            rb.gravityScale = originalGravityScale;
    }

    public void SetAutomaticDirection(float direction)
    {
        if (direction == 0f)
            return;

        automaticDirection = Mathf.Sign(direction);

        if (!touchingClimbingWall && horizontalInput == 0f)
            FaceDirection(automaticDirection);
    }

    private void FaceDirection(float direction)
    {
        if (direction == 0f)
            return;

        bool facingLeft = direction < 0f;

        if (spriteRenderer != null)
            spriteRenderer.flipX = facingLeft;

        Vector2 newOffset = originalColliderOffset;

        if (facingLeft)
            newOffset.x = -originalColliderOffset.x;

        playerCollider.offset = newOffset;

        if (swingVisualRenderer != null)
            swingVisualRenderer.flipX = facingLeft;
    }

    private bool HasTagInParents(Transform target, string tagName)
    {
        Transform current = target;

        while (current != null)
        {
            if (current.CompareTag(tagName))
                return true;

            current = current.parent;
        }

        return false;
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        if (HasTagInParents(collision.transform, climbingWallTag))
        {
            bool touchingSide = false;

            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (Mathf.Abs(contact.normal.x) > 0.7f)
                {
                    touchingSide = true;
                    break;
                }
            }

            if (touchingSide)
            {
                climbingWallContacts.Add(collision.collider);
                touchingClimbingWall = true;
            }
        }

        if (HasTagInParents(collision.transform, groundTag))
        {
            foreach (ContactPoint2D contact in collision.contacts)
            {
                if (contact.normal.y > 0.7f)
                {
                    groundContacts.Add(collision.collider);
                    isGrounded = true;
                    break;
                }
            }
        }
    }

    private void OnCollisionExit2D(Collision2D collision)
    {
        climbingWallContacts.Remove(collision.collider);
        touchingClimbingWall = climbingWallContacts.Count > 0;

        groundContacts.Remove(collision.collider);
        isGrounded = groundContacts.Count > 0;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryAttachToSwing(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryAttachToSwing(other);
    }

    private void TryAttachToSwing(Collider2D other)
    {
        if (isSwinging || Keyboard.current == null)
            return;

        if (!Keyboard.current.spaceKey.isPressed)
            return;

        Transform wall = other.transform;

        while (wall != null && !wall.CompareTag(swingWallTag))
            wall = wall.parent;

        if (wall == null)
            return;

        // Each SwingWall collider supplies its own anchor.
        Bounds bounds = other.bounds;

        swingAnchorPosition = new Vector2(
            bounds.center.x,
            bounds.max.y
        );

        AttachToSwing();
    }

    private void AttachToSwing()
    {
        Vector2 difference =
            (Vector2)transform.position - swingAnchorPosition;

        swingRadius = difference.magnitude;

        if (swingRadius < 0.1f)
            return;

        isSwinging = true;

        swingAngle = Mathf.Atan2(
            difference.x,
            -difference.y
        );

        // Start without artificial sideways movement.
        // Gravity will naturally accelerate the swing.
        swingAngularVelocity = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(true);

        Debug.Log("Player attached to gravity-driven swing!");
    }

    private void HandleSwingMovement()
    {
        // Pendulum acceleration caused by gravity.
        float angularAcceleration =
            -(swingGravity / swingRadius) *
            Mathf.Sin(swingAngle);

        swingAngularVelocity +=
            angularAcceleration * Time.fixedDeltaTime;

        // Small damping prevents endless swinging.
        swingAngularVelocity *=
            Mathf.Clamp01(1f - swingDamping * Time.fixedDeltaTime);

        swingAngle +=
            swingAngularVelocity * Time.fixedDeltaTime;

        Vector2 newPosition = swingAnchorPosition + new Vector2(
            Mathf.Sin(swingAngle) * swingRadius,
            -Mathf.Cos(swingAngle) * swingRadius
        );

        rb.MovePosition(newPosition);

        // The visual keeps its original position, rotation and scale.
    }

    private void ReleaseSwing()
    {
        if (!isSwinging)
            return;

        isSwinging = false;
        rb.gravityScale = originalGravityScale;

        // Tangent direction of the player's current swing.
        Vector2 launchDirection = new Vector2(
            Mathf.Cos(swingAngle),
            Mathf.Sin(swingAngle)
        );

        launchDirection *= Mathf.Sign(swingAngularVelocity);

        if (launchDirection.sqrMagnitude > 0.001f)
        {
            launchDirection.Normalize();

            float launchSpeed =
                swingAngularVelocity * swingRadius + swingLaunchBoost;

            launchSpeed = Mathf.Max(launchSpeed, swingLaunchBoost);

            rb.linearVelocity = launchDirection * launchSpeed;
        }

        swingLaunchControlTimer = swingLaunchControlDuration;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);

        Debug.Log("Player launched from swing!");
    }
}
