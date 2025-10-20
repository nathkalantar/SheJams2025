using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private Animator animator;
    [SerializeField] private SpriteRenderer spriteRenderer;
    
    private CharacterController controller;
    private Vector3 moveDirection;
    private bool facingRight = false;

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
        
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (direction.magnitude >= 0.1f)
        {
            moveDirection = direction;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
        
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