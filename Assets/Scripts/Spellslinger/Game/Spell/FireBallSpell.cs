using UnityEngine;
using UnityEngine.VFX;

namespace Spellslinger.Game.Spell
{
    public class FireBallSpell : MonoBehaviour {
        [SerializeField] private ParticleSystem onCollisionParticleSystem;

        private float speed = 8.0f;

        public Vector3 SpellDirection { get; set; }

        private void Awake() {
            this.SpellDirection = Vector3.forward;
        }

        private void Update() {
            this.transform.position += this.SpellDirection * this.speed * Time.deltaTime;
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Inflammable")) {
                // Check if the object has a Visual Effect component and enable it if not already enabled
                VisualEffect visualEffect = other.gameObject.GetComponent<VisualEffect>();
                if (visualEffect != null) {
                    if (!visualEffect.enabled) {
                        visualEffect.enabled = true;
                        other.gameObject.transform.GetChild(0).gameObject.SetActive(true);
                    }
                }
            }
        }

        private void OnCollisionEnter(Collision collision) {
            Instantiate(this.onCollisionParticleSystem, this.transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
    }
}
