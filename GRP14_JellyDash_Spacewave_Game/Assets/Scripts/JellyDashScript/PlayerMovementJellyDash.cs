
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
    [SerializeField] private float swingLaunchBoost = 3f;
    [SerializeField] private float maxSwingAttachDistance = 3f;

    private Rigidbody2D rb;
    private BoxCollider2D playerCollider;

    private Vector2 originalColliderOffset;
    private float originalGravityScale;

    private float automaticDirection = 1f;
    private float horizontalInput;

    private bool isGrounded;
    private bool touchingClimbingWall;
    private bool isSwinging;

    private readonly HashSet<Collider2D> climbingWallContacts =
        new HashSet<Collider2D>();

    private readonly HashSet<Collider2D> groundContacts =
        new HashSet<Collider2D>();

    private Transform swingAnchor;

    private float swingRadius;
    private float swingAngle;
    private float swingAngularVelocity;

    private void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<BoxCollider2D>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        originalColliderOffset = playerCollider.offset;
        originalGravityScale = rb.gravityScale;

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;
        rb.freezeRotation = true;

        // Player faces left by default.
        FaceDirection(-1f);

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);
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

        // Turn the sprite when pressing A or D.
        if (horizontalInput != 0f && !isSwinging)
            FaceDirection(horizontalInput);

        // Swing release.
        if (isSwinging)
        {
            if (Keyboard.current.spaceKey.wasReleasedThisFrame)
                ReleaseSwing();

            return;
        }

        // Normal ground jump.
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

        // DIRECT A AND D WALL CLIMBING.
        if (touchingClimbingWall)
        {
            if (horizontalInput != 0f)
            {
                // Holding A or D moves the player diagonally upwards.
                rb.gravityScale = 0f;

                rb.linearVelocity = new Vector2(
                    horizontalInput * climbingHorizontalSpeed,
                    climbingVerticalSpeed
                );

                FaceDirection(horizontalInput);
            }
            else
            {
                // Stop climbing when neither key is held.
                rb.gravityScale = originalGravityScale;

                float verticalSpeed = rb.linearVelocity.y;

                if (verticalSpeed < 0f)
                {
                    verticalSpeed = Mathf.Max(
                        verticalSpeed,
                        -wallSlideSpeed
                    );
                }

                rb.linearVelocity = new Vector2(
                    0f,
                    verticalSpeed
                );
            }

            return;
        }

        // NORMAL MOVEMENT OUTSIDE THE CLIMBING WALL.
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
        {
            rb.gravityScale =
                originalGravityScale * fallGravityMultiplier;
        }
        else
        {
            rb.gravityScale = originalGravityScale;
        }
    }

    public void SetAutomaticDirection(float direction)
    {
        if (direction == 0f)
            return;

        automaticDirection = Mathf.Sign(direction);

        if (!touchingClimbingWall && horizontalInput == 0f)
            FaceDirection(automaticDirection);
    }

    // Flip the sprite and mirror the original collider offset.
    private void FaceDirection(float direction)
    {
        if (spriteRenderer == null ||
            playerCollider == null ||
            direction == 0f)
            return;

        bool facingLeft = direction < 0f;

        spriteRenderer.flipX = facingLeft;

        Vector2 newOffset = originalColliderOffset;

        if (facingLeft)
            newOffset.x = -originalColliderOffset.x;

        playerCollider.offset = newOffset;
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
        // CLIMBING WALL DETECTION.
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

        // GROUND DETECTION.
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

        Vector2 anchor = new Vector2(
            other.bounds.center.x,
            other.bounds.max.y
        );

        float distance = Vector2.Distance(
            transform.position,
            anchor
        );

        if (distance > maxSwingAttachDistance)
            return;

        AttachToSwing(wall, anchor);
    }

    private void AttachToSwing(Transform wall, Vector2 anchor)
    {
        isSwinging = true;
        swingAnchor = wall;

        Vector2 difference =
            (Vector2)transform.position - anchor;

        swingRadius = difference.magnitude;

        if (swingRadius < 0.01f)
            swingRadius = 0.01f;

        swingAngle = Mathf.Atan2(
            difference.x,
            -difference.y
        );

        swingAngularVelocity = 0f;

        rb.linearVelocity = Vector2.zero;
        rb.gravityScale = 0f;

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(true);
    }

    private void HandleSwingMovement()
    {
        if (swingAnchor == null)
        {
            ReleaseSwing();
            return;
        }

        float angularAcceleration =
            -(swingGravity / swingRadius) *
            Mathf.Sin(swingAngle);

        swingAngularVelocity +=
            angularAcceleration * Time.fixedDeltaTime;

        swingAngle +=
            swingAngularVelocity * Time.fixedDeltaTime;

        Vector2 anchorPosition = swingAnchor.position;

        Vector2 newPosition = anchorPosition + new Vector2(
            Mathf.Sin(swingAngle) * swingRadius,
            -Mathf.Cos(swingAngle) * swingRadius
        );

        rb.MovePosition(newPosition);

        if (swingVisual != null)
        {
            swingVisual.position = anchorPosition;

            Vector2 direction = newPosition - anchorPosition;

            float angle =
                Mathf.Atan2(direction.y, direction.x) *
                Mathf.Rad2Deg;

            swingVisual.rotation =
                Quaternion.Euler(0f, 0f, angle - 90f);

            Vector3 scale = swingVisual.localScale;
            scale.y = direction.magnitude;
            swingVisual.localScale = scale;
        }
    }

    private void ReleaseSwing()
    {
        if (!isSwinging)
            return;

        isSwinging = false;
        rb.gravityScale = originalGravityScale;

        if (swingAnchor != null)
        {
            Vector2 launchDirection =
                (Vector2)transform.position -
                (Vector2)swingAnchor.position;

            if (launchDirection.sqrMagnitude > 0f)
                launchDirection.Normalize();

            rb.linearVelocity = new Vector2(
                launchDirection.x * swingLaunchBoost,
                Mathf.Max(
                    launchDirection.y * swingLaunchBoost,
                    jumpForce * 0.5f
                )
            );
        }

        if (swingVisual != null)
            swingVisual.gameObject.SetActive(false);

        swingAnchor = null;
    }
}
