using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Transform cameraTransform; // Referencia a la cámara
    
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
            
        // Si no se asignó la cámara manualmente, buscar la cámara principal
        if (cameraTransform == null)
        {
            Camera mainCamera = Camera.main;
            if (mainCamera != null)
                cameraTransform = mainCamera.transform;
        }
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
        
        // Crear vector de entrada
        Vector3 inputDirection = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (inputDirection.magnitude >= 0.1f && cameraTransform != null)
        {
            // Obtener la dirección hacia adelante y derecha de la cámara (ignorando rotación Y)
            Vector3 cameraForward = cameraTransform.forward;
            Vector3 cameraRight = cameraTransform.right;
            
            // Proyectar las direcciones de la cámara en el plano horizontal (Y = 0)
            cameraForward.y = 0f;
            cameraRight.y = 0f;
            cameraForward.Normalize();
            cameraRight.Normalize();
            
            // Calcular dirección de movimiento relativa a la cámara
            Vector3 moveDirection = cameraForward * inputDirection.z + cameraRight * inputDirection.x;
            moveDirection.Normalize();
            
            // Mover el personaje
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
            
            // Guardar la dirección para el flip
            this.moveDirection = moveDirection;
        }
        else if (inputDirection.magnitude >= 0.1f)
        {
            // Fallback: movimiento absoluto si no hay cámara
            Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
            moveDirection = direction;
            // Update lastMoveDirection for NPCs
            lastMoveDirection = direction;
            controller.Move(moveDirection * moveSpeed * Time.deltaTime);
        }
        
        // Aplicar gravedad
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
        // Usar la dirección de movimiento real (relativa a la cámara) para el flip
        if (moveDirection.magnitude > 0.1f)
        {
            // Determinar dirección basada en el componente X del movimiento mundial
            bool shouldFaceRight = moveDirection.x > 0;
            
            if (shouldFaceRight && !facingRight)
            {
                Flip();
            }
            else if (!shouldFaceRight && facingRight)
            {
                Flip();
            }
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

