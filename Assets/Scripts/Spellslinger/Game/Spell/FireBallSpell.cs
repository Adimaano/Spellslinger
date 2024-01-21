namespace Spellslinger.Game.Spell
{
    using UnityEngine;
    using UnityEngine.VFX;

    public class FireBallSpell : GenericSpell {
        [SerializeField] private ParticleSystem onCollisionParticleSystem;

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
        
        public void Explode() {
            Instantiate(this.onCollisionParticleSystem, this.transform.position, Quaternion.identity);
            Destroy(this.gameObject);
        }
        
        private new void OnCollisionEnter(Collision collision) {
            this.Explode();
        }
    }
}
