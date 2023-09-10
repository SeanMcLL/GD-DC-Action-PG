using UnityEngine;
using UnityEngine.InputSystem;
namespace GDDC
{
    public class PlayerMovementConstant : MonoBehaviour
    {
        public Rigidbody2D rb;
        public Transform groundCheck;
        public LayerMask groundLayer;
        private CharacterController2D m_Controller;
        
        private float horizontal;
        public float speed;
        public float jumpingPower;
        private bool isFacingRight = true;
        public bool isGroundedChecker;

        void Update()
        {
            if (!isFacingRight && horizontal > 0f)
            {
                Flip();
            }
            else if (isFacingRight && horizontal < 0f)
            {
                Flip();
            }
        }

        private void FixedUpdate()
        {
            rb.velocity = new Vector2(horizontal * speed, rb.velocity.y);
            isGroundedChecker = IsGrounded();
        }

        public void Jump(InputAction.CallbackContext context)
        {
            if (context.performed && IsGrounded())
            {
                rb.velocity = new Vector2(rb.velocity.x, jumpingPower);
            }

            if (context.canceled && rb.velocity.y > 0f)
            {
                rb.velocity = new Vector2(rb.velocity.x, 0);
            }
        }
        
        private bool IsGrounded()
        {
            return Physics2D.OverlapCircle(groundCheck.position, 0.2f, groundLayer);
        }
        

        private void Flip()
        {
            isFacingRight = !isFacingRight;
            Vector3 localScale = transform.localScale;
            localScale.x *= -1f;
            transform.localScale = localScale;
        }

        public void Move(InputAction.CallbackContext context)
        {
            horizontal = context.ReadValue<Vector2>().x;
        }
    }
}