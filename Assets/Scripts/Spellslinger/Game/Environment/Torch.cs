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
        private Light light;
        private float lightIntensity;

        public bool IsLit { get; private set; }

        // Events
        public System.Action OnTorchLit { get; internal set; }

        private void Awake() {
            this.fire = this.transform.Find("Fire").GetComponent<VisualEffect>();
            this.light = this.transform.Find("Fire").Find("Point Light").GetComponent<Light>();
            this.lightIntensity = this.light.intensity;
            this.audioSource = this.GetComponent<AudioSource>();

            if (this.initiallyLit) {
                this.fire.Play();
                this.light.intensity = this.lightIntensity;
                this.IsLit = true;
            } else {
                this.fire.Stop();
                this.light.intensity = 0f;
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
        /// Coroutine to turn on the point light by gradually increasing its intensity.
        /// </summary>
        private IEnumerator LightTorchCoroutine() {
            float fadeDuration = .3f;
            float elapsedTime = 0f;

            while (elapsedTime < fadeDuration) {
                this.light.intensity = Mathf.Lerp(0f, this.lightIntensity, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }
        }

        /// <summary>
        /// Extinguish the torch.
        /// </summary>
        public void ExtinguishTorch() {
            this.fire.Stop();
            this.IsLit = false;
            this.light.intensity = 0f;
        }

        /// <summary>
        /// Light the torch.
        /// </summary>
        public void LightTorch() {
            this.PlayRandomFireIgniteSound();
            this.fire.Play();
            this.IsLit = true;
            this.OnTorchLit?.Invoke();

            StartCoroutine(this.LightTorchCoroutine());
        }
    }
}
