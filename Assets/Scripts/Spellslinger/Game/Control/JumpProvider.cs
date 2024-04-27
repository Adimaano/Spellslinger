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
        [SerializeField] private float jumpForce = 500.0f;
        [SerializeField] private LayerMask groundLayers;

        private XROrigin _xrRig;
        private CapsuleCollider _collider;
        private Rigidbody _body;
        private bool _isGrounded => Physics.Raycast( new Vector2(this.transform.position.x, this.transform.position.y + 2.0f), Vector3.down, 3.0f);

        private float gravity = Physics.gravity.y;
        private Vector3 movement;
        // Start is called before the first frame update
        // Update is called once per frame
        private void Start()
        {
            _xrRig = GetComponent<XROrigin>();
            _collider = GetComponent<CapsuleCollider>();
            _body = GetComponent<Rigidbody>();
            m_JumpAction.action.performed += OnJump;
        }
        private void Update()
        {
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
