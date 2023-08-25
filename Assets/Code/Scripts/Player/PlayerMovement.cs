using System;

using UnityEngine;

namespace GDDC
{
    [RequireComponent(typeof(CharacterController2D))]
    [DisallowMultipleComponent]
    public class PlayerMovement : MonoBehaviour
    {
        private CharacterController2D m_Controller;
        private Vector2 m_Velocity;

        private bool m_JumpLast;

        private float m_JumpVelocity => Mathf.Sqrt(Mathf.Abs(2 * m_JumpHeight * m_GravityScale * Physics2D.gravity.y));

        [SerializeField] private InputReader m_Input;

        [Header("Grounded Movement Settings")]
        [SerializeField, Tooltip("The maximum walking speed of the player")] private float m_MaxSpeed;
        [SerializeField, Tooltip("How much the player accelerates to the target speed")] private AnimationCurve m_AccelerationCurve;
        [SerializeField, Tooltip("The maximum acceleration")] private float m_MaxAcceleration;
        [SerializeField, Tooltip("A constant amount to decelerate by each frame when the target speed is 0")] private float m_Deceleration;

        [Header("Aerial Control Settings")]
        [SerializeField, Tooltip("How much gravity affects the player")] private float m_GravityScale = 1.0f;
        [SerializeField, Tooltip("How much extra gravity affects the player when falling")] private float m_FallMultiplier = 4.5f;
        [SerializeField, Tooltip("How much letting go of the jump button affects gravity")] private float m_LowJumpMultiplier = 2.5f;
        [SerializeField, Tooltip("How high the player jumps when holding the jump button down all the way")] private float m_JumpHeight = 3.0f;

        private void Awake()
        {
            m_Controller = GetComponent<CharacterController2D>();
            m_JumpLast = m_Input.Gameplay.Jump;
        }

        private void FixedUpdate()
        {
            float deltaTime = Time.fixedDeltaTime * Time.timeScale;

            Debug.Log($"{deltaTime} - {Time.unscaledDeltaTime}");

            float currentSpeed = m_Velocity.x;
            float targetSpeed = m_MaxSpeed * m_Input.Gameplay.Movement.x;

            if (targetSpeed != currentSpeed)
            {
                float currentT = Mathf.InverseLerp(0, Mathf.Abs(targetSpeed), Mathf.Abs(currentSpeed));
                float targetAcceleration = Mathf.Lerp(-m_MaxAcceleration, m_MaxAcceleration, m_AccelerationCurve.Evaluate(currentT));

                m_Velocity.x = Mathf.MoveTowards(currentSpeed, targetSpeed, targetAcceleration * deltaTime);
            }

            if (m_Controller.IsGrounded)
            {
                if (m_Input.Gameplay.Jump && !m_JumpLast)
                    m_Velocity.y = m_JumpVelocity;
                else
                {
                    m_Velocity.y = 0.0f;
                    Vector2 projection = m_Controller.LastGroundedHit.normal * Vector2.Dot(m_Velocity, m_Controller.LastGroundedHit.normal);
                    Vector2 perpendicular = m_Velocity - projection;
                    m_Velocity = perpendicular.normalized * m_Velocity.magnitude / m_Controller.LastGroundedHit.normal.y;
                }
            }
            else
            {
                float gravityScale = m_GravityScale;

                if (m_Velocity.y > 0.0f && !m_Input.Gameplay.Jump)
                    gravityScale *= m_LowJumpMultiplier;

                if (m_Velocity.y < 0.0f)
                    gravityScale *= m_FallMultiplier;

                m_Velocity.y += Physics2D.gravity.y * gravityScale * deltaTime;
            }

            m_Controller.Move(m_Velocity * deltaTime);

            m_JumpLast = m_Input.Gameplay.Jump;

            Time.timeScale = Mathf.Lerp(0.5f, 1.0f, Convert.ToSingle(!m_Input.Gameplay.Attack));
        }
    }
}
