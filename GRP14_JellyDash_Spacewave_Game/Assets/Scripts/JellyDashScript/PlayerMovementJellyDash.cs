using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMovementJellyDash : MonoBehaviour
{
    [Header("Movement")]
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float forwardPushSpeed = 5f;

    [Header("Jump")]
    [SerializeField] private float jumpForce = 10f;

    [Header("Gravity")]
    [SerializeField] private float fallGravityMultiplier = 1.5f;

    [Header("Ground")]
    [SerializeField] private string groundTag = "Ground";

    [Header("Sticky Wall")]
    [SerializeField] private string stickyWallTag = "StickyWall";
    [SerializeField] private float stickyStretchX = 0.65f;
    [SerializeField] private float stickyStretchY = 1.45f;

    [Header("Climbing Walls")]
    [SerializeField] private string climbingWallTag = "ClimbingWall";
    [SerializeField] private float climbUpSpeed = 5f;
    [SerializeField] private float climbSideSpeed = 4f;

    [Header("Visual")]
    [SerializeField] private float squashSpeed = 10f;

    private Rigidbody2D rb;

    private bool isGrounded;
    private bool touchingStickyWall;
    private bool touchingClimbingWall;

    private float horizontalInput;

    private Vector3 normalScale;
    private Vector3 targetVisualScale;

    private Transform visual;

    private void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        visual = transform.Find("Visual");

        if (visual != null)
        {
            normalScale = visual.localScale;
            targetVisualScale = normalScale;
        }
        else
        {
            normalScale = Vector3.one;
            targetVisualScale = normalScale;
        }
    }

    private void Update()
    {
        GetMovementInput();

        CheckJump();

        UpdateVisual();
    }

    private void FixedUpdate()
    {
        if (touchingClimbingWall)
        {
            HandleClimbingMovement();
        }
        else
        {
            HandleNormalMovement();
            ApplyGravity();
        }
    }

    private void GetMovementInput()
    {
        horizontalInput = 0f;

        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.aKey.isPressed)
        {
            horizontalInput = -1f;
        }

        if (Keyboard.current.dKey.isPressed)
        {
            horizontalInput = 1f;
        }
    }

    private void HandleNormalMovement()
    {
        float horizontalMovement = horizontalInput * moveSpeed;

        if (horizontalInput == 0f)
        {
            horizontalMovement = moveSpeed;
        }

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            rb.linearVelocity.y
        );
    }

    private void HandleClimbingMovement()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        bool pressingW = Keyboard.current.wKey.isPressed;
        bool pressingA = Keyboard.current.aKey.isPressed;
        bool pressingD = Keyboard.current.dKey.isPressed;

        float horizontalMovement = 0f;
        float verticalMovement = 0f;

        if (pressingA)
        {
            horizontalMovement = -climbSideSpeed;
        }

        if (pressingD)
        {
            horizontalMovement = climbSideSpeed;
        }

        if (pressingW)
        {
            verticalMovement = climbUpSpeed;
        }

        rb.linearVelocity = new Vector2(
            horizontalMovement,
            verticalMovement
        );
    }

    private void CheckJump()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            if (isGrounded)
            {
                PerformNormalJump();
            }
        }
    }

    private void PerformNormalJump()
    {
        rb.linearVelocity = new Vector2(
            rb.linearVelocity.x,
            jumpForce
        );

        isGrounded = false;
    }

    private void ApplyGravity()
    {
        if (rb.linearVelocity.y < 0f)
        {
            rb.linearVelocity += Vector2.up *
                Physics2D.gravity.y *
                (fallGravityMultiplier - 1f) *
                Time.fixedDeltaTime;
        }
    }

    private void OnCollisionEnter2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag(stickyWallTag))
        {
            touchingStickyWall = true;

            targetVisualScale = new Vector3(
                stickyStretchX,
                stickyStretchY,
                normalScale.z
            );

            Debug.Log("Touching StickyWall");
        }

        if (collision.gameObject.CompareTag(climbingWallTag))
        {
            touchingClimbingWall = true;

            Debug.Log("Touching ClimbingWall");
        }

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

    private void OnCollisionExit2D(
        Collision2D collision)
    {
        if (collision.gameObject.CompareTag(stickyWallTag))
        {
            touchingStickyWall = false;
            targetVisualScale = normalScale;
        }

        if (collision.gameObject.CompareTag(climbingWallTag))
        {
            touchingClimbingWall = false;
        }

        if (collision.gameObject.CompareTag(groundTag))
        {
            isGrounded = false;
        }
    }

    private void UpdateVisual()
    {
        if (visual == null)
        {
            return;
        }

        if (!touchingStickyWall)
        {
            targetVisualScale = normalScale;
        }

        visual.localScale = Vector3.Lerp(
            visual.localScale,
            targetVisualScale,
            squashSpeed * Time.deltaTime
        );
    }
}