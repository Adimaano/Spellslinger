using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class IntroManager : MonoBehaviour {
    [Header("Initially Disabled Objects")]
    [SerializeField] private GameObject[] initiallyDisabledObjects;

    [Header("Initially Enabled Objects")]
    [SerializeField] private GameObject[] initiallyEnabledObjects;

    [Header("Torches")]
    [SerializeField] private GameObject firstTorchGO;
    [SerializeField] private Light spotlightFirstTorch;
    [SerializeField] private Torches lastTorch;
    private Torches firstTorch;
    private float firstTorchIntensity;
    private bool firstTorchLit = false;
    private bool roomIsLit = false;

    [Header("Exit Door")]
    [SerializeField] private Animator doorAnimator;
    [SerializeField] private AudioSource doorAudioSource;

    private bool bookTriggered = false;

    private void Start() {
        // Disable all initially disabled objects
        for (int i = 0; i < this.initiallyDisabledObjects.Length; i++) {
            this.initiallyDisabledObjects[i].SetActive(false);
        }

        this.firstTorchIntensity = this.spotlightFirstTorch.intensity;
        this.spotlightFirstTorch.intensity = 0.0f;
        this.firstTorch = firstTorchGO.transform.Find("TargetTorch").Find("Torch").GetComponent<Torches>();
    }

    private void Update() {
        if (!this.firstTorchLit && this.firstTorch.isLit) {
            // Enable all initially disabled objects
            for (int i = 0; i < this.initiallyDisabledObjects.Length; i++) {
                this.initiallyDisabledObjects[i].SetActive(true);
            }

            // Disable all initially enabled objects
            for (int i = 0; i < this.initiallyEnabledObjects.Length; i++) {
                this.initiallyEnabledObjects[i].SetActive(false);
            }

            // Set Background Type of Main Camera to Skybox
            Camera.main.clearFlags = CameraClearFlags.Skybox;

            this.firstTorchLit = true;
        }

        if(!this.roomIsLit && this.lastTorch.isLit) {
            StartCoroutine(OpenDoor());

            this.roomIsLit = true;
        }
    }

    private IEnumerator SpotlightFirstTorch(Light spotlight, float maxIntensity) {
        while (spotlight.intensity < maxIntensity) {
            
            spotlight.intensity += maxIntensity/100;
            yield return new WaitForSeconds(0.05f);
        }
    }

    private IEnumerator OpenDoor() {
        this.doorAnimator.SetTrigger("openDoor");
        yield return new WaitForSeconds(0.15f);
        doorAudioSource.Play();
    }

    private void OnTriggerEnter(Collider other) {
        if (!this.bookTriggered && other.CompareTag("Player")) {
            this.firstTorchGO.SetActive(true);
            StartCoroutine(SpotlightFirstTorch(this.spotlightFirstTorch, this.firstTorchIntensity));
            this.bookTriggered = true;
        }
    }
}
