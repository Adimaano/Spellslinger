using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Spellslinger.Game;
using Spellslinger.Game.Environment;

public class IntroManager : MonoBehaviour {
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

    [Header("First Torch")]
    [SerializeField] private Torch firstTorch;
    [SerializeField] private Light spotlightFirstTorch;
    private Light firstTorchPointLight;
    private float initialfirstTorchSpotlightIntensity;
    private float firstTorchPointLightInitialIntensity;

    [Header("Torches")]
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

    private bool bookTriggered = false;

    private void Start() {
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
            StartCoroutine(OnFirstTorchLit());
        };
        
        // Eventlisteners First Puzzle (lit both standing torches)
        foreach(Torch torch in this.floorTorches) {
            torch.OnTorchLit = () => {
                this.numberOfFloorTorchesLit++;
                if (this.numberOfFloorTorchesLit == this.floorTorches.Length) {
                    StartCoroutine(this.OpenDoor());
                }
            };
        }

        // Setup second puzzle
        this.puzzleTorchesLitOrder = new int[this.puzzleTorches.Length];

        // Eventlisteners Second Puzzle (lit torches in correct order)
        for(int i = 0; i < this.puzzleTorches.Length; i++) {
            int index = i;
            this.puzzleTorches[i].OnTorchLit = () => {
                this.PuzzleTorchLit(index);
            };
        }
    }

    /// <summary>
    /// Coroutine that reveals the room. Light Fade Effect and Enabling of Game Objects.
    /// </summary>
    private IEnumerator OnFirstTorchLit() {
        StartCoroutine(this.LightDomeFadeEffect());
        GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);

        yield return new WaitForSeconds(this.sceneFadeDuration + 0.5f);

        this.torches[0].LightTorch();
        GameManager.Instance.PlayAudioClip(this.igniteSound);

        for(int i = 1; i < torches.Length; i += 2) {
            yield return new WaitForSeconds(this.timeBetweenTorches);
            this.torches[i].LightTorch();
            this.torches[i].gameObject.GetComponent<AudioSource>().PlayOneShot(this.igniteSound);
            this.torches[i+1].LightTorch();
            this.torches[i+1].gameObject.GetComponent<AudioSource>().PlayOneShot(this.igniteSound);
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
            spotlight.intensity += maxIntensity/100;
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
                Debug.Log("Puzzle solved");
                GameManager.Instance.PlayAudioClip(this.puzzleSolvedSound);
            } else {
                // Reset puzzle and play sound
                this.puzzleTorchesLit = 0;
                this.puzzleTorchesLitOrder = new int[this.puzzleTorches.Length];
                GameManager.Instance.PlayAudioClip(this.puzzleFailSound);

                foreach (Torch torch in this.puzzleTorches) {
                    torch.ExtinguishTorch();
                }
            }

        }

    }

    /// <summary>
    /// Opens the exit door and plays the door opening sound.
    /// </summary>
    private IEnumerator OpenDoor() {
        this.doorAnimator.SetTrigger("openDoor");
        yield return new WaitForSeconds(0.15f);
        doorAudioSource.Play();
    }

    private void OnTriggerEnter(Collider other) {
        if (!this.bookTriggered && other.CompareTag("Player")) {
            this.firstTorch.gameObject.transform.parent.gameObject.SetActive(true);
            StartCoroutine(SpotlightFirstTorch(this.spotlightFirstTorch, this.initialfirstTorchSpotlightIntensity));
            this.bookTriggered = true;
        }
    }
}
