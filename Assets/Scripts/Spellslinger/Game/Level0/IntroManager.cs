namespace Spellslinger.Game {
    using System.Collections;
    using System.Linq;
    using Spellslinger.AI;
    using Spellslinger.Game.Control;
    using Spellslinger.Game.Environment;
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class IntroManager : MonoBehaviour {
        private ModelRunner modelRunner;

        [Header("Initially Disabled Objects")]
        [SerializeField] private GameObject[] initiallyDisabledObjects;

        [Header("Initially Enabled Objects")]
        [SerializeField] private GameObject[] initiallyEnabledObjects;

        [Header("Light Fade Effects")]
        [SerializeField] private Material volumetricLightInitialMaterial;
        [SerializeField] private Material fadeInCylinderMaterial;
        private Color fadeInCylinderMaterialColor;
        private float volumetricLightInitialMaterialAlpha;
        private float fadeInCylinderIntialMaterialAlpha;

        [Header("First Spell")]
        [SerializeField] private Torch firstTorch;
        [SerializeField] private Light spotlightFirstTorch;
        [SerializeField] private GameObject drawRuneExample;
        private Light firstTorchPointLight;
        private float initialfirstTorchSpotlightIntensity;
        private float firstTorchPointLightInitialIntensity;

        [Header("First Puzzle")]
        [SerializeField] private Torch[] torches;
        [SerializeField] private Torch[] floorTorches;
        private int numberOfFloorTorchesLit = 0;

        [Header("Exit Door")]
        [SerializeField] private Animator doorAnimator;
        [SerializeField] private AudioSource doorAudioSource;

        [Header("Sounds")]
        [SerializeField] private AudioClip igniteSound;
        [SerializeField] private AudioClip puzzleSolvedSound;
        [SerializeField] private AudioClip puzzleFailSound;

        [Header("Scene Settings")]
        [Tooltip("The duration of the fade in and fade out effect after the player hits the frist torch.")]
        [SerializeField] private float sceneFadeDuration = 1.5f;
        [Tooltip("The time between the wallTorches lighting up after the room is revealed.")]
        [SerializeField] private float timeBetweenTorches = 1.5f;

        [Header("Second Puzzle")]
        [SerializeField] private Torch[] puzzleTorches;
        private int[] puzzleTorchesLitOrder;
        private int puzzleTorchesLit = 0;
        [SerializeField] private Animator exitDoorAnimator;
        [SerializeField] private AudioClip exitDoorOpenSound;

        [Header("Portal Room")]
        [SerializeField] private GameObject portal;
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Material portalMaterialDefault;
        [SerializeField] private GameObject[] portalRoomTorches;
        private Color portalMaterialColor;
        private float portalActivationDuration = 1.0f;

        [Header("Wizard Voice")]
        [SerializeField] private AudioSource wizardVoiceAudioSource;
        [SerializeField] private AudioClip wizardVoiceIntro;
        [SerializeField] private AudioClip wizardVoiceTeleportHint;
        [SerializeField] private AudioClip wizardVoiceExplainCharging;
        [SerializeField] private AudioClip wizardVoiceExplainCasting;
        [SerializeField] private AudioClip wizardVoiceLightsOnSpeech;
        [SerializeField] private AudioClip wizardVoiceFirstPuzzleHintOne;
        [SerializeField] private AudioClip wizardVoiceFirstPuzzleHintTwo;
        [SerializeField] private AudioClip wizardVoiceFirstPuzzleHintThree;
        [SerializeField] private AudioClip wizardVoiceSecondPuzzleHint;
        [SerializeField] private AudioClip wizardVoiceSecondPuzzleFailedHintOne;
        [SerializeField] private AudioClip wizardVoiceSecondPuzzleFailedHintTwo;
        private float wizardVoiceHintTimer = 15.0f;
        private int puzzleHintsPlayed = 0;
        private bool failedSecondPuzzle = false;

        private bool bookTriggered = false;
        private bool isFirstTorchesLit = false;

        private void Start() {
            // find dependencies in scene
            this.modelRunner = GameObject.Find("-- XR --").GetComponent<ModelRunner>();

            // Disable all initially disabled objects
            for (int i = 0; i < this.initiallyDisabledObjects.Length; i++) {
                this.initiallyDisabledObjects[i].SetActive(false);
            }

            // Get initial intensity of spotlight for first and then disable it
            this.initialfirstTorchSpotlightIntensity = this.spotlightFirstTorch.intensity;
            this.spotlightFirstTorch.intensity = 0.0f;

            // get point lights of first torch
            this.firstTorchPointLight = this.firstTorch.transform.Find("Fire").Find("Point Light").GetComponent<Light>();
            this.firstTorchPointLightInitialIntensity = this.firstTorchPointLight.intensity;

            // Get initial alpha values of fade effects
            this.volumetricLightInitialMaterialAlpha = 0.4f;
            this.volumetricLightInitialMaterial.SetFloat("_Alpha", this.volumetricLightInitialMaterialAlpha);

            // get initial alpha value of base map of material
            this.fadeInCylinderMaterialColor = this.fadeInCylinderMaterial.GetColor("_BaseColor");
            this.fadeInCylinderIntialMaterialAlpha = 0.0f;

            // Event Listeners
            this.firstTorch.OnTorchLit = () => {
                this.StartCoroutine(this.OnFirstTorchLit());
            };

            // Eventlisteners First Puzzle (lit both standing torches)
            foreach (Torch torch in this.floorTorches) {
                torch.OnTorchLit = () => {
                    this.numberOfFloorTorchesLit++;
                    this.wizardVoiceHintTimer = Time.time + 60.0f;
                    if (this.numberOfFloorTorchesLit == this.floorTorches.Length) {
                        this.StartCoroutine(this.OpenDoor());
                        this.puzzleHintsPlayed = 0;
                    }
                };
            }

            Color baseEmissionColor = this.portalMaterialDefault.GetColor("_EmissionColor");
            this.portalMaterial.SetColor("_EmissionColor", baseEmissionColor);

            // Setup second puzzle
            this.puzzleTorchesLitOrder = new int[this.puzzleTorches.Length];

            // Eventlisteners Second Puzzle (lit torches in correct order)
            for (int i = 0; i < this.puzzleTorches.Length; i++) {
                int index = i;
                this.puzzleTorches[i].OnTorchLit = () => {
                    this.PuzzleTorchLit(index);
                };
            }

            this.StartCoroutine(this.PlayWizardVoiceDelayed(this.wizardVoiceIntro, 3.5f));
            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceIntro.length + 20.0f;

            // Eventlistener
            this.modelRunner.OnPredictionReceived += this.SpellCasted;
        }

        private void Update() {
            if (Time.time > this.wizardVoiceHintTimer) {
                if (!this.bookTriggered) {
                    this.PlayWizardVoice(this.wizardVoiceTeleportHint);
                    this.wizardVoiceHintTimer = Time.time + this.wizardVoiceTeleportHint.length + 30.0f;
                } else if (this.isFirstTorchesLit && this.numberOfFloorTorchesLit != this.floorTorches.Length) {
                    switch (this.puzzleHintsPlayed) {
                        case 0:
                            this.PlayWizardVoice(this.wizardVoiceFirstPuzzleHintOne);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceFirstPuzzleHintOne.length + 60.0f;
                            this.puzzleHintsPlayed++;
                            break;
                        case 1:
                            this.PlayWizardVoice(this.wizardVoiceFirstPuzzleHintTwo);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceFirstPuzzleHintTwo.length + 45.0f;
                            this.puzzleHintsPlayed++;
                            break;
                        case 2:
                            this.PlayWizardVoice(this.wizardVoiceFirstPuzzleHintThree);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceFirstPuzzleHintThree.length + 45.0f;
                            break;
                    }
                } else if (this.isFirstTorchesLit && !this.failedSecondPuzzle) {
                    switch (this.puzzleHintsPlayed) {
                        case 0:
                            this.PlayWizardVoice(this.wizardVoiceSecondPuzzleHint);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceSecondPuzzleHint.length + 50.0f;
                            this.puzzleHintsPlayed++;
                            break;
                        case 1:
                            this.PlayWizardVoice(this.wizardVoiceFirstPuzzleHintOne);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceFirstPuzzleHintOne.length + 60.0f;
                            this.puzzleHintsPlayed++;
                            break;
                        case 2:
                            this.PlayWizardVoice(this.wizardVoiceFirstPuzzleHintTwo);
                            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceFirstPuzzleHintTwo.length + 60.0f;
                            break;
                    }
                }
            }
        }

        /// <summary>
        /// Coroutine that plays an audioclip for the wizard after a delay.
        /// </summary>
        /// <param name="clip">The audioclip to play.</param>
        private IEnumerator PlayWizardVoiceDelayed(AudioClip clip, float delay) {
            yield return new WaitForSeconds(delay);
            this.PlayWizardVoice(clip);
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
        /// Coroutine that reveals the room. Light Fade Effect and Enabling of Game Objects.
        /// </summary>
        private IEnumerator OnFirstTorchLit() {
            this.StartCoroutine(this.LightDomeFadeEffect());
            this.wizardVoiceAudioSource.Stop();
            GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);

            yield return new WaitForSeconds(this.sceneFadeDuration + 0.75f);

            this.torches[0].LightTorch();
            this.isFirstTorchesLit = true;
            GameManager.Instance.PlayAudioClip(this.igniteSound, .7f);
            this.StartCoroutine(this.PlayWizardVoiceDelayed(this.wizardVoiceLightsOnSpeech, .5f));
            this.wizardVoiceHintTimer = Time.time + this.wizardVoiceLightsOnSpeech.length + 60.0f;

            for (int i = 1; i < this.torches.Length; i += 2) {
                yield return new WaitForSeconds(this.timeBetweenTorches);
                this.torches[i].LightTorch();
                this.torches[i].gameObject.GetComponent<AudioSource>().PlayOneShot(this.igniteSound, .7f);
                this.torches[i + 1].LightTorch();
                this.torches[i + 1].gameObject.GetComponent<AudioSource>().PlayOneShot(this.igniteSound, .7f);
            }
        }

        /// <summary>
        /// Coroutine that fades in (and out) the volumetric light and the cylinder.
        /// </summary>
        private IEnumerator LightDomeFadeEffect() {
            float elapsedTime = 0f;

            // Fade volumetric light to alpha 0 and fade in cylinder to alpha 1 over 1 second
            while (elapsedTime < this.sceneFadeDuration) {
                // fade out volumetric light (cone in the middle of the room)
                this.volumetricLightInitialMaterial.SetFloat("_Alpha", Mathf.Lerp(this.volumetricLightInitialMaterialAlpha, 0.0f, elapsedTime / this.sceneFadeDuration));

                // fade in cylinder (shortly blacken everything around the player)
                float fadeInCylinderAlpha = Mathf.Lerp(this.fadeInCylinderIntialMaterialAlpha, 1.0f, elapsedTime / this.sceneFadeDuration);
                this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, fadeInCylinderAlpha);
                this.fadeInCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);

                // fade out spotlight of first torch
                this.spotlightFirstTorch.intensity = Mathf.Lerp(this.initialfirstTorchSpotlightIntensity, 0.0f, elapsedTime / this.sceneFadeDuration);
                this.firstTorchPointLight.intensity = Mathf.Lerp(this.firstTorchPointLightInitialIntensity, 0.0f, elapsedTime / this.sceneFadeDuration);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.volumetricLightInitialMaterial.SetFloat("_Alpha", 0.0f);
            this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, 255);
            this.fadeInCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);

            yield return new WaitForSeconds(0.5f);

            // Set Background Type of Main Camera to Skybox
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            // Enable all initially disabled objects
            for (int i = 0; i < this.initiallyDisabledObjects.Length; i++) {
                this.initiallyDisabledObjects[i].SetActive(true);
            }

            // Disable all initially enabled objects
            for (int i = 0; i < this.initiallyEnabledObjects.Length; i++) {
                this.initiallyEnabledObjects[i].SetActive(false);
            }

            elapsedTime = 0f;

            while (elapsedTime < this.sceneFadeDuration) {
                this.volumetricLightInitialMaterial.SetFloat("_Alpha", Mathf.Lerp(0.0f, this.volumetricLightInitialMaterialAlpha, elapsedTime / this.sceneFadeDuration));

                float fadeInCylinderAlpha = Mathf.Lerp(1.0f, this.fadeInCylinderIntialMaterialAlpha, elapsedTime / this.sceneFadeDuration);
                this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, fadeInCylinderAlpha);
                this.fadeInCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.volumetricLightInitialMaterial.SetFloat("_Alpha", this.volumetricLightInitialMaterialAlpha);
            this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, this.fadeInCylinderIntialMaterialAlpha);
            this.fadeInCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);
        }

        /// <summary>
        /// Coroutine that turns on the spotlight for the first torch by increasing the intensity of the spotlight.
        /// </summary>
        private IEnumerator SpotlightFirstTorch(Light spotlight, float maxIntensity) {
            while (spotlight.intensity < maxIntensity) {
                spotlight.intensity += maxIntensity / 100;
                yield return new WaitForSeconds(0.05f);
            }
        }

        private void PuzzleTorchLit(int torchIndex) {
            this.puzzleTorchesLitOrder[this.puzzleTorchesLit] = torchIndex;
            this.puzzleTorchesLit++;

            // All puzzle torches are lit
            if (this.puzzleTorchesLit == this.puzzleTorches.Length) {
                // Check if the puzzle was solved correctly puzzleTorchesLitOrder should be {0, 1, 2, 3}
                bool puzzleSolved = this.puzzleTorchesLitOrder.Select((value, index) => value == index).All(result => result);

                if (puzzleSolved) {
                    this.exitDoorAnimator.SetBool("isFenceDown", true);
                    this.exitDoorAnimator.gameObject.GetComponent<AudioSource>().PlayOneShot(this.exitDoorOpenSound);
                    GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);
                    this.StartCoroutine(this.ActivatePortal());
                } else {
                    // Reset puzzle and play sound
                    this.puzzleTorchesLit = 0;
                    this.puzzleTorchesLitOrder = new int[this.puzzleTorches.Length];
                    GameManager.Instance.PlayAudioClip(this.puzzleFailSound);

                    foreach (Torch torch in this.puzzleTorches) {
                        torch.ExtinguishTorch();
                    }

                    if (!this.failedSecondPuzzle) {
                        this.StartCoroutine(this.PlayWizardVoiceDelayed(this.wizardVoiceSecondPuzzleFailedHintOne, 1.5f));
                        this.wizardVoiceHintTimer = Time.time + this.wizardVoiceSecondPuzzleFailedHintOne.length + 60.0f;
                    } else {
                        this.StartCoroutine(this.PlayWizardVoiceDelayed(this.wizardVoiceSecondPuzzleFailedHintTwo, 1.5f));
                        this.wizardVoiceHintTimer = Time.time + this.wizardVoiceSecondPuzzleFailedHintTwo.length + 60.0f;
                    }

                    this.failedSecondPuzzle = true;
                }
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

        /// <summary>
        /// Opens the exit door and plays the door opening sound.
        /// </summary>
        private IEnumerator OpenDoor() {
            this.doorAnimator.SetTrigger("openDoor");
            yield return new WaitForSeconds(0.15f);
            this.doorAudioSource.Play();
        }

        /// <summary>
        /// Called when the AI Model has finished predicting a rune.
        /// </summary>
        /// <param name="runeClass">Class of the predicted rune.</param>
        private void SpellCasted(int runeClass) {
            if (this.bookTriggered && runeClass == 2) {
                this.firstTorch.gameObject.transform.parent.gameObject.SetActive(true);
                this.StartCoroutine(this.SpotlightFirstTorch(this.spotlightFirstTorch, this.initialfirstTorchSpotlightIntensity));
                this.PlayWizardVoice(this.wizardVoiceExplainCasting);
                this.wizardVoiceHintTimer = Time.time + this.wizardVoiceExplainCasting.length + 60.0f;
                this.modelRunner.OnPredictionReceived -= this.SpellCasted;
                this.drawRuneExample.SetActive(false);
            }
        }

        private void OnTriggerEnter(Collider other) {
            if (!this.bookTriggered && other.CompareTag("Player")) {
                GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Fire);
                this.PlayWizardVoice(this.wizardVoiceExplainCharging);
                this.wizardVoiceHintTimer = Time.time + this.wizardVoiceExplainCharging.length + 60.0f;
                this.drawRuneExample.SetActive(true);
                this.bookTriggered = true;
                this.wizardVoiceHintTimer = Time.time + 60.0f;
            }
        }
    }
}