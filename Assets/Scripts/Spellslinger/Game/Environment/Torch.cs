using System.Collections;
using System.Collections.Generic;
using UnityEngine.VFX;
using UnityEngine;

namespace Spellslinger.Game.Environment
{
    public class Torch : MonoBehaviour {
        [SerializeField] private AudioClip[] fireInginiteSounds;
        [SerializeField] private bool initiallyLit = false;

        private VisualEffect fire;
        private AudioSource audioSource;

        public bool IsLit { get; private set; }

        // Events
        public System.Action OnTorchLit { get; internal set; }

        private void Awake() {
            this.fire = this.transform.Find("Fire").GetComponent<VisualEffect>();
            this.audioSource = this.GetComponent<AudioSource>();

            if (this.initiallyLit) {
                this.fire.Play();
                this.IsLit = true;
            } else {
                this.fire.Stop();
                this.IsLit = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Fire" && !this.IsLit) {
                this.LightTorch();
            }
        }

        /// <summary>
        /// Play a random fire ignite sound.
        /// </summary>
        private void PlayRandomFireIgniteSound() {
            int index = Random.Range(0, this.fireInginiteSounds.Length);
            this.audioSource.clip = this.fireInginiteSounds[index];
            this.audioSource.Play();
        }

        /// <summary>
        /// Extinguish the torch.
        /// </summary>
        public void ExtinguishTorch() {
            this.fire.Stop();
            this.IsLit = false;
        }

        /// <summary>
        /// Light the torch.
        /// </summary>
        public void LightTorch() {
            this.PlayRandomFireIgniteSound();
            this.fire.Play();
            this.IsLit = true;
            this.OnTorchLit?.Invoke();
        }
    }
}
