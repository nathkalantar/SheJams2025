using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private float moveSpeed = 5f;
    [SerializeField] private float rotationSpeed = 720f;
    
    private CharacterController controller;
    private Vector3 moveDirection;

    void Start()
    {
        controller = GetComponent<CharacterController>();
        if (controller == null)
            controller = gameObject.AddComponent<CharacterController>();
    }

    void Update()
    {
        HandleMovement();
    }

    void HandleMovement()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");
        
        Vector3 direction = new Vector3(horizontal, 0f, vertical).normalized;
        
        if (direction.magnitude >= 0.1f)
        {
            float targetAngle = Mathf.Atan2(direction.x, direction.z) * Mathf.Rad2Deg;
            float angle = Mathf.LerpAngle(transform.eulerAngles.y, targetAngle, rotationSpeed * Time.deltaTime / 360f);
            transform.rotation = Quaternion.AngleAxis(angle, Vector3.up);
            
            moveDirection = Quaternion.AngleAxis(targetAngle, Vector3.up) * Vector3.forward;
            controller.Move(moveDirection.normalized * moveSpeed * Time.deltaTime);
        }
        
        controller.Move(Vector3.down * 9.81f * Time.deltaTime);
    }
}