using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float runSpeed = 8f; // Velocidad al correr
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;

    private CharacterController controller;
    private Vector3 moveDirection;
    private bool facingRight = false;
    private bool isRunning = false; // Para detectar si está corriendo

    // Public property for NPCs to access the player's last movement direction
    public Vector3 lastMoveDirection { get; private set; } = Vector3.forward;

    // Flag to track if movement sound is playing (prevents spamming one-shot sounds)
    private bool isMovingSoundPlaying = false;

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

        // Detectar si se está presionando Shift para correr
        isRunning = Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift);
        Debug.Log("Playerisrunning");

        // Elegir velocidad según si está corriendo o caminando
        float currentSpeed = isRunning ? runSpeed : moveSpeed;

        // Get camera-relative directions (flattened to XZ plane for 2.5D movement)
        Vector3 cameraForward = Camera.main.transform.forward;
        cameraForward.y = 0f;  // Ignore Y to keep movement on ground
        cameraForward.Normalize();

        Vector3 cameraRight = Camera.main.transform.right;
        cameraRight.y = 0f;  // Ignore Y
        cameraRight.Normalize();

        // Calculate movement direction relative to camera
        Vector3 direction = (cameraRight * horizontal + cameraForward * vertical).normalized;

        bool isMoving = direction.magnitude >= 0.1f;

        if (isMoving)
        {
            moveDirection = direction;
            // Update lastMoveDirection for NPCs
            lastMoveDirection = direction;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);

            // Play one-shot movement sound if not already playing
            if (!isMovingSoundPlaying && AudioManager.instance != null)
            {
                Debug.Log("PlayerController: Calling PlayWalk()");  // Debug: Check if this appears
                AudioManager.instance.PlayWalk();  // One-shot sound for movement start
                isMovingSoundPlaying = true;
            }
            else if (AudioManager.instance == null)
            {
                Debug.LogWarning("PlayerController: AudioManager.instance is null");  // Debug: Check for null instance
            }
        }
        else
        {
            // If not moving, keep last direction
            moveDirection = Vector3.zero;

            // Stop the movement sound if it was playing
            if (isMovingSoundPlaying && AudioManager.instance != null)
            {
                Debug.Log("PlayerController: Calling StopWalk()");  // Debug: Check if this appears
                AudioManager.instance.StopWalk();  // Stop the sound immediately
                isMovingSoundPlaying = false;
            }
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
        animator.SetBool("isRunning", isRunning && isWalking); // Solo corre si se está moviendo

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



