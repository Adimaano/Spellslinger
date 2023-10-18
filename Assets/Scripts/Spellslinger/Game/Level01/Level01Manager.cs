namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Environment;
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class Level01Manager : MonoBehaviour {
        [Header("First Puzzle | Fence")]
        [SerializeField] private Torch fenceTorch;
        [SerializeField] private Animator fenceAnimator;
        [SerializeField] private AudioClip fenceOpenSound;

        [Header("Second Puzzle | Mirrors")]
        [SerializeField] private Torch[] mirrorTorches;
        [SerializeField] private Torch wrongMirrorTorch;
        [SerializeField] private GameObject[] mirrorTorchesGO;
        [SerializeField] private GameObject wrongMirrorTorchGO;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Transform playerOrigin;
        private bool isMirrorPuzzleSolved = false;
        private int walkedInCircles = 0;

        // const with offset to mirror room
        private const float roomOffsetX = 29.5f;
        private const float roomOffsetY = 0.0f;
        private const float roomOffsetZ = -30f;

        [Header("Portal Room")]
        [SerializeField] private GameObject portal;
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Material portalMaterialDefault;
        [SerializeField] private GameObject[] portalRoomTorches;
        private Color portalMaterialColor;
        private float portalActivationDuration = 1.0f;

        [Header("Sounds")]
        [SerializeField] private AudioClip puzzleSolvedSound;
        [SerializeField] private AudioClip puzzleFailSound;


        [Header("Wizard Voice")]
        [SerializeField] private AudioSource wizardVoiceAudioSource;
        [SerializeField] private AudioClip thatsNotRight;
        [SerializeField] private AudioClip mirrorPuzzleHint01;
        [SerializeField] private AudioClip mirrorPuzzleHint02;
        [SerializeField] private AudioClip mirrorPuzzleHint03;
        [SerializeField] private AudioClip mirrorPuzzleHint04;
        [SerializeField] private AudioClip mirrorPuzzleSolved;
        [SerializeField] private AudioClip walkedInCircles1;
        [SerializeField] private AudioClip walkedInCircles2;
        [SerializeField] private AudioClip walkedInCircles3;
        [SerializeField] private AudioClip walkedInCircles100;
        [SerializeField] private AudioClip paintingOldPetunia;
        [SerializeField] private AudioClip paintingHaplessPercival;
        [SerializeField] private AudioClip paintingLucius;
        [SerializeField] private AudioClip paintingCedric;
        private float wizardVoiceHintTimer = 15.0f;
        private int wizardVoiceHintCounter = 0;

        private bool paintingOldPetuniaTriggered = false;
        private bool paintingHaplessPercivalTriggered = false;
        private bool paintingLuciusTriggered = false;
        private bool paintingCedricTriggered = false;

        private void Start() {
            this.fenceTorch.OnTorchLit = () => {
                this.OnFenceTorchLit();
            };

            this.wrongMirrorTorch.OnTorchLit = () => {
                this.OnWrongMirrorTorchLit();
            };

            foreach (Torch torch in this.mirrorTorches) {
                torch.OnTorchLit = () => {
                    this.OnMirrorTorchLit();
                };
            }

            Color baseEmissionColor = this.portalMaterialDefault.GetColor("_EmissionColor");
            this.portalMaterial.SetColor("_EmissionColor", baseEmissionColor);
        }

        private void Update() {
            if (this.isMirrorPuzzleSolved || this.walkedInCircles < 1) {
                return;
            }

            if (Time.time > this.wizardVoiceHintTimer) {
                if (this.wizardVoiceHintCounter == 0) {
                    this.wizardVoiceHintCounter++;
                    this.wizardVoiceHintTimer = Time.time + 30.0f + this.mirrorPuzzleHint01.length;
                    this.StartCoroutine(this.PlayWizardVoiceNext(this.mirrorPuzzleHint01));
                } else if (this.wizardVoiceHintCounter == 1) {
                    this.wizardVoiceHintCounter++;
                    this.wizardVoiceHintTimer = Time.time + 30.0f + this.mirrorPuzzleHint02.length;
                    this.StartCoroutine(this.PlayWizardVoiceNext(this.mirrorPuzzleHint02));
                } else if (this.wizardVoiceHintCounter == 2) {
                    this.wizardVoiceHintCounter++;
                    this.wizardVoiceHintTimer = Time.time + 30.0f + this.mirrorPuzzleHint03.length;
                    this.StartCoroutine(this.PlayWizardVoiceNext(this.mirrorPuzzleHint03));
                } else {
                    this.wizardVoiceHintTimer = Time.time + 30.0f + this.mirrorPuzzleHint04.length;
                    this.StartCoroutine(this.PlayWizardVoiceNext(this.mirrorPuzzleHint04));
                }
            }
        }

        /// <summary>
        /// Coroutine that plays an audioclip for the wizard after a delay.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        /// <param name="delay">The delay before the audioclip is played.</param>
        private IEnumerator PlayWizardVoiceDelayed(AudioClip clip, float delay) {
            yield return new WaitForSeconds(delay);
            this.PlayWizardVoice(clip);
        }

        /// <summary>
        /// Coroutine that plays an audioclip for the wizard after the current one has finished.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        /// <param name="delay">The delay after the current audioclip has finished.</param>
        private IEnumerator PlayWizardVoiceNext(AudioClip clip, float delay = 0.5f) {
            while (this.wizardVoiceAudioSource.isPlaying) {
                yield return null;
            }

            this.StartCoroutine(this.PlayWizardVoiceDelayed(clip, delay));
        }

        /// <summary>
        /// Plays an audioclip for the wizard.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private void PlayWizardVoice(AudioClip clip) {
            this.wizardVoiceAudioSource.Stop();
            this.wizardVoiceAudioSource.PlayOneShot(clip);
        }

        /// <summary>
        /// Called when the fence torch is lit. Lowers the fence.
        /// </summary>
        private void OnFenceTorchLit() {
            this.fenceAnimator.SetBool("isFenceDown", true);
            this.fenceAnimator.gameObject.GetComponent<AudioSource>().PlayOneShot(this.fenceOpenSound);
        }

        /// <summary>
        /// Called when a mirror torch is lit. Checks if all torches are lit.
        /// </summary>
        private void OnMirrorTorchLit() {
            if (this.isMirrorPuzzleSolved) {
                return;
            }

            // check if all torches are lit
            foreach (Torch torch in this.mirrorTorches) {
                if (!torch.IsLit) {
                    return;
                }
            }

            foreach (GameObject torch in this.mirrorTorchesGO) {
                torch.SetActive(true);
            }

            this.wrongMirrorTorch.enabled = false;

            // puzzle solved
            this.isMirrorPuzzleSolved = true;
            this.GetComponent<BoxCollider>().enabled = false;
            GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);
            this.StartCoroutine(this.PlayWizardVoiceDelayed(this.mirrorPuzzleSolved, .5f));
            this.StartCoroutine(this.ActivatePortal());
        }

        /// <summary>
        /// Called when the wrong mirror torch is lit. Extinguishes all torches.
        /// </summary>
        private void OnWrongMirrorTorchLit() {
            if (this.isMirrorPuzzleSolved) {
                this.wrongMirrorTorch.ExtinguishTorch();
                return;
            }

            GameManager.Instance.PlayAudioClip(this.puzzleFailSound);
            this.wrongMirrorTorch.ExtinguishTorch();

            // extinguish all torches
            foreach (Torch torch in this.mirrorTorches) {
                torch.ExtinguishTorch();
            }

            this.StartCoroutine(this.PlayWizardVoiceNext(this.thatsNotRight));
        }

        /// <summary>
        /// Sets the player position to first room that looks exactly the same without the player knowing.
        /// </summary>
        private void ResetPlayerPosition() {
            this.walkedInCircles++;

            this.playerHead.position = new Vector3(this.playerHead.position.x - roomOffsetX, this.playerHead.position.y - roomOffsetY, this.playerHead.position.z - roomOffsetZ);
            this.playerOrigin.position = new Vector3(this.playerOrigin.position.x - roomOffsetX, this.playerOrigin.position.y - roomOffsetY, this.playerOrigin.position.z - roomOffsetZ);

            if (this.walkedInCircles == 1) {
                this.StartCoroutine(this.PlayWizardVoiceNext(this.walkedInCircles1));
                this.wizardVoiceHintTimer = Time.time + 30.0f + this.walkedInCircles1.length;
            } else if (this.walkedInCircles == 2) {
                this.wizardVoiceHintTimer = Time.time + 25.0f + this.walkedInCircles2.length;
                this.StartCoroutine(this.PlayWizardVoiceNext(this.walkedInCircles2));
            } else if (this.walkedInCircles == 3) {
                this.wizardVoiceHintTimer = Time.time + 25.0f + this.walkedInCircles3.length;
                this.StartCoroutine(this.PlayWizardVoiceNext(this.walkedInCircles3));
            } else if (this.walkedInCircles == 100) {
                this.StartCoroutine(this.PlayWizardVoiceNext(this.walkedInCircles100));
            }
        }

        /// <summary>
        /// Triggers the wizard to say something about a painting.
        /// </summary>
        public void TriggerPaintingSpeech(string paintingName, Transform paintingPosition) {
            if (this.wizardVoiceAudioSource.isPlaying) {
                return;
            }

            // check if the player is looking in the direction of the painting
            Vector3 playerDirection = this.playerHead.forward;
            Vector3 playerToPainting = paintingPosition.position - this.playerHead.position;
            float angle = Vector3.Angle(playerDirection, playerToPainting);

            if (angle > 35f) {
                return;
            }

            if (paintingName == "oldPetunia" && !this.paintingOldPetuniaTriggered) {
                this.StartCoroutine(this.DisablePaintingSpeechTemporarily(paintingName));
                this.PlayWizardVoice(this.paintingOldPetunia);
            } else if (paintingName == "haplessPercival" && !this.paintingHaplessPercivalTriggered) {
                this.StartCoroutine(this.DisablePaintingSpeechTemporarily(paintingName));
                this.PlayWizardVoice(this.paintingHaplessPercival);
            } else if (paintingName == "lucius" && !this.paintingLuciusTriggered) {
                this.StartCoroutine(this.DisablePaintingSpeechTemporarily(paintingName));
                this.PlayWizardVoice(this.paintingLucius);
            } else if (paintingName == "cedric" && !this.paintingCedricTriggered) {
                this.StartCoroutine(this.DisablePaintingSpeechTemporarily(paintingName));
                this.PlayWizardVoice(this.paintingCedric);
            }
        }

        private IEnumerator DisablePaintingSpeechTemporarily(string paintingName) {
            if (paintingName == "oldPetunia") {
                this.paintingOldPetuniaTriggered = true;
            } else if (paintingName == "haplessPercival") {
                this.paintingHaplessPercivalTriggered = true;
            } else if (paintingName == "lucius") {
                this.paintingLuciusTriggered = true;
            } else if (paintingName == "cedric") {
                this.paintingCedricTriggered = true;
            }

            yield return new WaitForSeconds(15.0f);

            if (paintingName == "oldPetunia") {
                this.paintingOldPetuniaTriggered = false;
            } else if (paintingName == "haplessPercival") {
                this.paintingHaplessPercivalTriggered = false;
            } else if (paintingName == "lucius") {
                this.paintingLuciusTriggered = false;
            } else if (paintingName == "cedric") {
                this.paintingCedricTriggered = false;
            }
        }

        /// <summary>
        /// Coroutine that activates the portal. Enables the portal GameObject, fades the emission
        /// intensity of the portal material to 7 and lights all torches.
        /// </summary>
        private IEnumerator ActivatePortal() {
            yield return new WaitForSeconds(1.5f);
            foreach (GameObject torch in this.portalRoomTorches) {
                torch.SetActive(true);
            }

            yield return new WaitForSeconds(1.5f);
            this.portal.SetActive(true);
            this.portal.transform.parent.gameObject.GetComponent<Portal>().IsActive = true;

            float elapsedTime = 0.0f;
            Color baseEmissionColor = this.portalMaterial.GetColor("_EmissionColor");

            while (elapsedTime < this.portalActivationDuration) {
                elapsedTime += Time.deltaTime;
                float currentIntensity = Mathf.Lerp(0.0f, 60.0f, elapsedTime / this.portalActivationDuration);

                this.portalMaterialColor = baseEmissionColor * currentIntensity;
                this.portalMaterial.SetColor("_EmissionColor", this.portalMaterialColor);

                yield return null;
            }

            // Set the final intensity value
            Color finalEmissionColor = baseEmissionColor * 60.0f;
            this.portalMaterial.SetColor("_EmissionColor", finalEmissionColor);
        }

        private void OnTriggerEnter(Collider other) {
            if (other.CompareTag("Player") && !this.isMirrorPuzzleSolved) {
                this.ResetPlayerPosition();
            }
        }
    }
}