using UnityEngine;

public class SpriteDirectionalController : MonoBehaviour
{
    [Range(0f, 180f)][SerializeField] float backAngle = 65f;
    [Range(0f, 180f)][SerializeField] float sideAngle = 155f;
    [SerializeField] Transform maintransform;
    [SerializeField] Animator animator;
    [SerializeField] SpriteRenderer spriterenderer;

    private void Update()
    {
        Vector3 camForwardVector = new Vector3(Camera.main.transform.forward.x, 0f, Camera.main.transform.forward.z);
        Debug.DrawRay(Camera.main.transform.position, camForwardVector * 5f, Color.magenta);

        float signedAngle = Vector3.SignedAngle(maintransform.forward, camForwardVector, Vector3.up);

        Vector2 animationDirection = new Vector2(0F, -1F);

        float angle = Mathf.Abs(signedAngle);


        if (angle < backAngle)
        {
            animationDirection = new Vector2(0f, -1f);
        }
        else if (angle < backAngle)
        {

            //Changes the side animation based on what side the camera is viewing
            if (signedAngle < 0)
            {
                animationDirection = new Vector2(-1f, 0f);
            }
            else
            {
                animationDirection = new Vector2(1f, 0f);
            }
        }
        else
        {
            animationDirection = new Vector2(0f, 1f);
        }

        animator.SetFloat("moveX", animationDirection.x);
        animator.SetFloat("moveY", animationDirection.y);


    }
}
