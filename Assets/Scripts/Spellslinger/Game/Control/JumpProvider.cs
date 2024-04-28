namespace Spellslinger.Game.Control
{
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using UnityEngine.InputSystem;
    using Unity.XR.CoreUtils;
    
    public class JumpProvider : MonoBehaviour
    {
        [SerializeField] private InputActionProperty m_JumpAction;
        [SerializeField] private float jumpForce = 0.01f;
        [SerializeField] private LayerMask groundLayers;
        [SerializeField] private Transform groundCheckPoint;

        private XROrigin _xrRig;
        private CapsuleCollider _collider;
        private Rigidbody _body;
        //private bool _isGrounded => Physics.Raycast( new Vector3(this.transform.position.x, this.transform.position.y + 1.8f, this.transform.position.z), Vector3.down, 1.8f);
        private bool _isGrounded => Physics.OverlapSphere(groundCheckPoint.position, .25f, groundLayers).Length > 0;
        private void Start()
        {
            _xrRig = GetComponent<XROrigin>();
            _collider = GetComponent<CapsuleCollider>();
            _body = GetComponent<Rigidbody>();
            m_JumpAction.action.performed += OnJump;
        }
        private void FixedUpdate()
        {
            //Debug.DrawRay(new Vector3(this.transform.position.x, this.transform.position.y + 1.8f, this.transform.position.z), Vector3.down * 1.8f, Color.red, duration: 0, depthTest: true);
            var center = _xrRig.CameraInOriginSpacePos;
            _collider.height = Mathf.Clamp(_xrRig.CameraInOriginSpaceHeight, 1.0f, 3.0f);
            _collider.center = new Vector3(center.x, _collider.height / 2, center.z);
        }

        private void OnJump(InputAction.CallbackContext context)
        {   
            if(!_isGrounded)
            {
                Debug.Log("Not Grounded!");
                return;
            }
            _body.AddForce(Vector3.up * jumpForce, ForceMode.Impulse);
        }
    }
}
