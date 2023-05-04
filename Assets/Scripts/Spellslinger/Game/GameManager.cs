using System.Collections.Generic;
using Spellslinger.Game.Control;
using Spellslinger.Game.XR;
using UnityEngine;

namespace Spellslinger.Game
{
    public class GameManager : MonoBehaviour {
        private AudioSource soundEffectSource;
        private AudioSource musicSource;

        private XRInputManager input;
        private Player player;

        private bool isPaused = false;

        [SerializeField] private GameObject pauseMenuPrefab;

        [Header("Audio")]
        [SerializeField] private AudioClip[] soundEffects;

        // A dictionary to map sound effect names to audio clips
        private Dictionary<string, AudioClip> soundEffectDictionary;

        public static GameManager Instance { get; private set; }

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

        // Start is called before the first frame update
        private void Start() {
            this.input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();
            this.player = GameObject.Find("-- XR --").GetComponent<Player>();
            this.soundEffectSource = this.transform.Find("Sounds").GetComponent<AudioSource>();
            this.musicSource = this.transform.Find("Music").GetComponent<AudioSource>();

            // Initialize the sound effect dictionary
            this.soundEffectDictionary = new Dictionary<string, AudioClip>();
            for (int i = 0; i < this.soundEffects.Length; i++) {
                this.soundEffectDictionary.Add(this.soundEffects[i].name, this.soundEffects[i]);
            }

            // Initialize event listeners
            this.input.OnControllerMenu += this.PauseGame;
        }

        public void PauseGame() {
            if (this.isPaused) {
                Time.timeScale = 1.0f;

                this.isPaused = false;
                this.pauseMenuPrefab.SetActive(false);
            } else {
                Time.timeScale = 0.0f;

                this.isPaused = true;
                this.pauseMenuPrefab.SetActive(true);

                // set pause menu position to 2.5 meters in front of player
                Vector3 lookDirection = Camera.main.transform.forward;
                this.pauseMenuPrefab.transform.position = Camera.main.transform.position + (lookDirection * 2.5f);

                // set pause menu rotation to look at player but keep it upright
                Quaternion lookRotation = Quaternion.LookRotation(Camera.main.transform.position - this.pauseMenuPrefab.transform.position);
                this.pauseMenuPrefab.transform.rotation = Quaternion.Euler(0.0f, lookRotation.eulerAngles.y, 0.0f);
            }
        }

        public void SwitchPreferredController(string controller) {
            if (controller == "left") {
                PlayerPrefs.SetInt("preferredController", 0);

                this.player.PreferredController = XRInputManager.Controller.Left;
                this.input.SetPreferredController(XRInputManager.Controller.Left);
            } else if (controller == "right") {
                PlayerPrefs.SetInt("preferredController", 1);

                this.player.PreferredController = XRInputManager.Controller.Right;
                this.input.SetPreferredController(XRInputManager.Controller.Right);
            }
        }

        public void PlaySound(string soundName, float volume = 1.0f) {
            AudioClip clip = this.soundEffectDictionary[soundName];
            this.soundEffectSource.PlayOneShot(clip, volume);
        }
    }
}
