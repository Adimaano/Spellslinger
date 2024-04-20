namespace Spellslinger.Game.Control
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;

    public class JumpProvider : MonoBehaviour
    {
        [SerializeField] private InputActionProperty m_JumpAction;
        [SerializeField] private float jumpHeight = 3.0f;
        [SerializeField] private CharacterController m_CharacterController;
        [SerializeField] private LayerMask groundLayers;

        private float gravity = Physics.gravity.y;
        private Vector3 movement;
        // Start is called before the first frame update
        // Update is called once per frame
        private void Update()
        {
            bool _isGrounded = Physics.CheckSphere(m_CharacterController.transform.position, m_CharacterController.radius, groundLayers);

            if(m_JumpAction.action.triggered && _isGrounded)
            {
                movement.y = Mathf.Sqrt(jumpHeight * -2.0f * gravity);
            }

            movement.y += gravity * Time.deltaTime;

            m_CharacterController.Move(movement * Time.deltaTime);
        }
    }

}
