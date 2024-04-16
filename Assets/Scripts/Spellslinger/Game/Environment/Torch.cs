namespace Spellslinger.Game.Environment
{
    using System.Collections;
    using UnityEngine;
    // using UnityEngine.VFX;

    public class Torch : MonoBehaviour {
        [SerializeField] private AudioClip[] fireInginiteSounds;
        [SerializeField] private bool initiallyLit = false;

        private ParticleSystem fire;
        private ParticleSystem ember;
        private ParticleSystem smoke;
        private ParticleSystem.EmissionModule fireEmission;
        private ParticleSystem.EmissionModule emberEmission;
        private ParticleSystem.EmissionModule smokeEmission;
        private AudioSource audioSource;
        private Light lightSource;
        private float lightIntensity;

        public bool IsLit { get; private set; }

        // Events
        public System.Action OnTorchLit { get; internal set; }

        private void Awake() {
            // this.fire = this.transform.Find("Fire").GetComponent<VisualEffect>();
            // NOTE: switched to common particle system instead of VFX for performance reasons
            this.fire = this.transform.Find("Fire").GetComponent<ParticleSystem>();
            this.ember = this.transform.Find("Fire").Find("Ember").GetComponent<ParticleSystem>();
            this.smoke = this.transform.Find("Fire").Find("Smoke").GetComponent<ParticleSystem>();
            this.fireEmission = this.fire.emission;
            this.emberEmission = this.ember.emission;
            this.smokeEmission = this.smoke.emission;
            this.lightSource = this.transform.Find("Fire").Find("Point Light").GetComponent<Light>();
            this.lightIntensity = this.lightSource.intensity;
            this.audioSource = this.GetComponent<AudioSource>();

            if (this.initiallyLit) {
                this.fireEmission.enabled = true;
                this.emberEmission.enabled = true;
                this.smokeEmission.enabled = true;
                this.lightSource.intensity = this.lightIntensity;
                this.IsLit = true;
            } else {
                this.fireEmission.enabled = false;
                this.emberEmission.enabled = false;
                this.smokeEmission.enabled = false;
                this.lightSource.intensity = 0f;
                this.IsLit = false;
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.tag == "Fire" && !this.IsLit) {
                this.LightTorch();
            } else if (other.gameObject.tag == "Water" && this.IsLit) {
                this.ExtinguishTorch();
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
                this.lightSource.intensity = Mathf.Lerp(0f, this.lightIntensity, elapsedTime / fadeDuration);
                elapsedTime += Time.deltaTime;
                yield return null;
            }

            // In case torch was extinguished before coroutine finished
            if (!this.IsLit) {
                this.ExtinguishTorch();
            }
        }

        /// <summary>
        /// Extinguish the torch.
        /// </summary>
        public void ExtinguishTorch() {
            this.fireEmission.enabled = false;
            this.emberEmission.enabled = false;
            this.smokeEmission.enabled = false;

            this.IsLit = false;
            this.lightSource.intensity = 0f;
        }

        /// <summary>
        /// Light the torch.
        /// </summary>
        public void LightTorch() {
            this.PlayRandomFireIgniteSound();
            this.fireEmission.enabled = true;
            this.emberEmission.enabled = true;
            this.smokeEmission.enabled = true;
            this.IsLit = true;
            this.OnTorchLit?.Invoke();

            this.StartCoroutine(this.LightTorchCoroutine());
        }
    }
}
