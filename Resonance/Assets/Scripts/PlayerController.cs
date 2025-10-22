using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool facingRight = false;

    // Public property for NPCs to access the player's last movement direction
    public Vector3 lastMoveDirection { get; private set; } = Vector3.forward;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();

        if (animator == null)
            animator = GetComponent<Animator>();

        if (spriteRenderer == null)
            spriteRenderer = GetComponent<SpriteRenderer>();

        if (spriteRenderer != null)
            spriteRenderer.flipX = false;
    }

    void Update()
    {
        HandleMovement();
        HandleAnimations();
        HandleFlip();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        // Get camera-relative directions (flattened to XZ plane for 2.5D movement)
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;  // Ignore Y to keep movement on ground
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;  // Ignore Y
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 direction = (cameraRight * horizontal + cameraForward * vertical).normalized;

        if (direction.magnitude >= 0.1f)
        {
            moveDirection = direction;
            // Update lastMoveDirection for NPCs
            lastMoveDirection = direction;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        else
        {
            // If not moving, keep last direction
            moveDirection = Vector3.zero;
        }

        // Apply gravity (adjust 9.81f if your game has custom gravity)
        controller.Move(Vector3.down * 9.81f * Time.deltaTime);
    }

    void HandleAnimations()
    {
        if (animator == null) return;

        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        bool isWalking = Mathf.Abs(horizontal) > 0.1f || Mathf.Abs(vertical) > 0.1f;

        animator.SetBool("isWalking", isWalking);
    }

    void HandleFlip()
    {
        float horizontal = Input.GetAxis("Horizontal");

        if (horizontal > 0 && !facingRight)
        {
            Flip();
        }
        else if (horizontal < 0 && facingRight)
        {
            Flip();
        }
    }

    void Flip()
    {
        facingRight = !facingRight;

        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = facingRight;
        }
        else
        {
            Vector3 scale = transform.localScale;
            scale.x *= -1;
            transform.localScale = scale;
        }
    }
}

