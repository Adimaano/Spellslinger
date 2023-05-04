using UnityEngine;
using UnityEngine.VFX;

namespace Spellslinger.Game.Environment
{
    public class Torch : MonoBehaviour {
        private VisualEffect fire;
        [SerializeField] private bool isLit = false;

        private void Start() {
            this.fire = this.transform.Find("Fire").GetComponent<VisualEffect>();

            if (this.isLit) {
                this.LightTorch();
            } else {
                this.ExtinguishTorch();
            }
        }

        private void LightTorch() {
            this.isLit = true;
            this.fire.Play();
        }

        private void ExtinguishTorch() {
            this.isLit = false;
            this.fire.Stop();
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Fire" && !this.isLit) {
                this.LightTorch();
            }
        }
    }
}
