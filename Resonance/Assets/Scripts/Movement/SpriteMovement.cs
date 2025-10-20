using UnityEngine;

public class SpriteMovement : MonoBehaviour
{
    [SerializeField] private float speed = 3f;

    public Rigidbody playerRb;
    public float moveSpeed;

    private Vector2 moveInput;

    // Start is called before the first frame update
    void Start()
    {
       
    }

    // Update is called once per frame
    void Update()
    {
        float moveX = Input.GetAxisRaw("Horizontal");
        float moveY = Input.GetAxisRaw("Vertical");
        moveInput.Normalize();

        playerRb.velocity = new Vector3(moveInput.x * moveSpeed, playerRb.velocity.y, moveInput.y * moveSpeed);
    }

    private void FixedUpdate()
    {
        //playerRb.MovePosition(playerRb.position + moveInput * speed * Time.fixedDeltaTime);
    }


}
