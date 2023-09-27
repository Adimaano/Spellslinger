namespace Spellslinger.Game.Environment
{
    using Spellslinger.Game.Manager;
    using UnityEngine;
    using UnityEngine.XR.Interaction.Toolkit;

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

        private void OnEnable() {
            this.waitForCollisionDetection = Time.time + 0.5f;
        }

        // // Start is called before the first frame update
        private void Awake() {
            this.grabInteractable = this.gameObject.GetComponent<XRGrabInteractable>();

            if (this.grabInteractable == null) {
                this.grabInteractable = this.gameObject.AddComponent<XRGrabInteractable>();
            }

            this.rigidbodyComponent = this.gameObject.GetComponent<Rigidbody>();

            if (this.rigidbodyComponent == null) {
                this.rigidbodyComponent = this.gameObject.AddComponent<Rigidbody>();
            }

            this.audioSourceComponent = this.gameObject.GetComponent<AudioSource>();

            if (this.audioSourceComponent == null) {
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
                        int random = Random.Range(1, 10);
                        this.PlaySound("Glass0" + random);
                        break;
                    case ObjectType.Hard:
                        this.PlaySound("Hard01");
                        break;
                    case ObjectType.Soft:
                        this.PlaySound("Hard01");
                        break;
                    default:
                        this.PlaySound("Hard01");
                        break;
                }
            }
        }

        private void PlaySound(string soundName, float volume = 0.85f) {
            AudioClip clip = GameManager.Instance.GetAudioClipFromDictionary(soundName);
            this.audioSourceComponent.PlayOneShot(clip, 0.85f);
        }
    }
}
