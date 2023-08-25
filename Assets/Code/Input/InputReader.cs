using UnityEngine;
using UnityEngine.InputSystem;

namespace GDDC
{
    [CreateAssetMenu(menuName = "GDDC/Input/Input Reader")]
    public class InputReader : ScriptableObject, GameInputs.IGameplayActions
    {
        private GameInputs m_Inputs = null;

        public GameplayInputs Gameplay;

        public struct GameplayInputs
        {
            public Vector2 Movement;
            public bool Jump;
            public bool Attack;
            public bool Menu;
            public bool Interact;
        }

        private void OnEnable()
        {
            if (m_Inputs == null)
            {
                m_Inputs = new GameInputs();
                m_Inputs.Gameplay.SetCallbacks(this);
            }
            
            m_Inputs.Gameplay.Enable();
        }

        private void OnDisable()
        {
            if (m_Inputs != null)
                m_Inputs.Gameplay.Disable();
        }

        public void OnMovement(InputAction.CallbackContext context) => Gameplay.Movement = context.ReadValue<Vector2>();
        public void OnJump(InputAction.CallbackContext context) => Gameplay.Jump = context.ReadValueAsButton();
        public void OnAttack(InputAction.CallbackContext context) => Gameplay.Attack = context.ReadValueAsButton();
        public void OnMenu(InputAction.CallbackContext context) => Gameplay.Menu = context.ReadValueAsButton();
        public void OnInteract(InputAction.CallbackContext context) => Gameplay.Interact = context.ReadValueAsButton();
    }
}
