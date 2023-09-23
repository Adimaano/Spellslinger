using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Spellslinger.Game.Environment
{
    public class GrabbableObject : MonoBehaviour {
        private XRGrabInteractable grabInteractable;
        private Rigidbody rigidbodyComponent;
        private AudioSource audioSourceComponent;

        [SerializeField] private float objectMass = .5f;
        [SerializeField] private float throwVelocityScale = 1.5f;
        private float waitForCollisionDetection = 0.1f;
        [SerializeField] private ObjectType option;

        private enum ObjectType
        {
            Hard,
            Soft,
            Glass,
        }

        // // Start is called before the first frame update
        private void Awake() {
            this.grabInteractable = this.gameObject.GetComponent<XRGrabInteractable>();

            if (this.grabInteractable == null) {
                Debug.Log("Adding XRGrabInteractable to " + this.gameObject.name);
                this.grabInteractable = this.gameObject.AddComponent<XRGrabInteractable>();
            }

            this.rigidbodyComponent = this.gameObject.GetComponent<Rigidbody>();

            if (this.rigidbodyComponent == null) {
                Debug.Log("Adding rigidbody to " + this.gameObject.name);
                this.rigidbodyComponent = this.gameObject.AddComponent<Rigidbody>();
            }

            this.audioSourceComponent = this.gameObject.GetComponent<AudioSource>();

            if (this.audioSourceComponent == null) {
                Debug.Log("Adding audio source to " + this.gameObject.name);
                this.audioSourceComponent = this.gameObject.AddComponent<AudioSource>();
            }

            // set layer of grabInteractable
            this.grabInteractable.interactionLayers = InteractionLayerMask.GetMask("Grabbable");

            // set collision detection mode
            this.rigidbodyComponent.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

            // set object mass
            this.rigidbodyComponent.mass = this.objectMass;

            // set throw force
            this.grabInteractable.throwOnDetach = true;
            this.grabInteractable.throwVelocityScale = this.throwVelocityScale;

            // set audio source to 3D
            this.audioSourceComponent.spatialBlend = 1.0f;

            this.waitForCollisionDetection += Time.time;
        }

        private void OnCollisionEnter(Collision other) {
            if (Time.time < this.waitForCollisionDetection) {
                return;
            }

            if (!other.gameObject.CompareTag("Player")) {
                switch (this.option) {
                    case ObjectType.Glass:
                        this.audioSourceComponent.PlayOneShot(GameManager.Instance.GetAudioClipFromDictionary("impact_glass"), 0.85f);
                        break;
                    case ObjectType.Hard:
                        this.audioSourceComponent.PlayOneShot(GameManager.Instance.GetAudioClipFromDictionary("impact_short"), 0.85f);
                        break;
                    case ObjectType.Soft:
                        this.audioSourceComponent.PlayOneShot(GameManager.Instance.GetAudioClipFromDictionary("impact_soft"), 0.85f);
                        break;
                    default:
                        this.audioSourceComponent.PlayOneShot(GameManager.Instance.GetAudioClipFromDictionary("impact_short"), 0.85f);
                        break;
                }
                
            }
        }
    }
}
