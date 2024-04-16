namespace Spellslinger.Game.Manager
{
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Control;
    using Spellslinger.Game.XR;
    using UnityEngine;
    using UnityEngine.SceneManagement;

    public class GameManager : MonoBehaviour {
        private AudioSource soundEffectSource;
        private AudioSource musicSource;

        private XRInputManager input;
        private Player player;

        private bool isPaused = false;

        [SerializeField] private GameObject pauseMenuPrefab;

        // TODO: Maybe move the audio stuff to a separate class? Maybe a SoundManager?
        [Header("Audio")]
        [SerializeField] private AudioClip[] soundEffects;

        // A dictionary to map sound effect names to audio clips
        private Dictionary<string, AudioClip> soundEffectDictionary;

        [Header("Level Loading Effect")]
        [SerializeField] private Material fadeToWhiteCylinderMaterial;
        [SerializeField] private GameObject teleportationParticles;
        private Color fadeInCylinderMaterialColor;
        private float sceneFadeDuration = 2.0f;
        private float playerDeathFadeDuration = .5f;

        public static GameManager Instance { get; private set; }

        private void Awake() {
            if (Instance == null) {
                Instance = this;
            } else {
                Destroy(this.gameObject);
            }

            DontDestroyOnLoad(this.gameObject);
        }

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

            this.fadeInCylinderMaterialColor = this.fadeToWhiteCylinderMaterial.GetColor("_BaseColor");
            this.StartCoroutine(this.RemoveFadeCylinderOnStart());

            // Initialize event listeners
            this.input.OnControllerMenu += this.PauseGame;
        }

        /// <summary>
        /// Pauses the game by setting the timescale to 0.0f and enabling the pause menu.
        /// </summary>
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

        /// <summary>
        /// Switches the preferred controller by calling the XRInputManager's SetPreferredController method.
        /// This method is called by the buttons in the PauseMenu.
        /// </summary>
        /// <param name="controller">The name of the controller to switch to.</param>
        public void SwitchPreferredController(string controller) {
            if (controller == "left") {
                SaveData saveData = SaveGameManager.Instance.GetSaveData();
                saveData.preferredHand = XRInputManager.Controller.Left;
                SaveGameManager.Save(saveData);

                this.input.SetPreferredController(XRInputManager.Controller.Left);
            } else if (controller == "right") {
                SaveData saveData = SaveGameManager.Instance.GetSaveData();
                saveData.preferredHand = XRInputManager.Controller.Right;
                SaveGameManager.Save(saveData);

                this.input.SetPreferredController(XRInputManager.Controller.Right);
            }
        }

        /// <summary>
        /// Plays a sound by name.
        /// </summary>
        /// <param name="soundName">The name of the sound to play.</param>
        /// <param name="volume">[optional] The volume to play the sound at. Default: 1.0f.</param>
        public void PlaySound(string soundName, float volume = 1.0f) {
            AudioClip clip = this.soundEffectDictionary[soundName];
            this.soundEffectSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Play an audio clip.
        /// </summary>
        /// <param name="clip">The audio clip to play.</param>
        /// <param name="volume">[optional] The volume to play the audio clip at. Default: 1.0f.</param>
        public void PlayAudioClip(AudioClip clip, float volume = 1.0f) {
            this.soundEffectSource.PlayOneShot(clip, volume);
        }

        /// <summary>
        /// Load Level by name.
        /// </summary>
        /// <param name="levelName">The name of the level to load.</param>
        public void LoadLevel(string levelName) {
            SceneManager.LoadScene(levelName);
        }

        /// <summary>
        /// Restart the current level.
        /// </summary>
        public void ReloadLevel() {
            this.PauseGame();
            int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
            this.StartCoroutine(this.TeleportToNextLevel(currentSceneIndex));
        }

        /// <summary>
        /// Load the main menu.
        /// </summary>
        public void BackToMainMenu() {
            this.PauseGame();
            this.StartCoroutine(this.TeleportToNextLevel(0));
        }

        /// <summary>
        /// Load Level by index. Starts a coroutine that fades in the cylinder and then loads the level with the given index.
        /// </summary>
        /// <param name="levelIndex">The index of the level to load.</param>
        public void LoadLevel(int levelIndex) {
            this.StartCoroutine(this.TeleportToNextLevel(levelIndex));
        }

        /// <summary>
        /// Restart the current level. Is not actually a restart/reload but rather a reset of the player's position.
        /// </summary>
        public void RestartLevel() {
            this.StartCoroutine(this.ResetPlayerToSpawnPosition());
        }

        private IEnumerator ResetPlayerToSpawnPosition() {
            yield return this.StartCoroutine(this.FadeInTransitionCylinder(this.playerDeathFadeDuration));

            this.player.ResetPlayerToSpawnPosition();

            yield return this.StartCoroutine(this.FadeOutTransitionCylinder(this.sceneFadeDuration));
        }

        /// <summary>
        /// Ienumerator that fades in the cylinder and then loads the level with the given index.
        /// </summary>
        /// <param name="levelIndex">The index of the level to load.</param> 
        private IEnumerator TeleportToNextLevel(int levelIndex) {
            this.teleportationParticles.SetActive(true);

            yield return this.StartCoroutine(this.FadeInTransitionCylinder(this.sceneFadeDuration));

            SceneManager.LoadScene(levelIndex);
        }

        /// <summary>
        /// Ienumerator that fades out the cylinder. Gives the player a feel of teleportation.
        /// </summary>
        private IEnumerator RemoveFadeCylinderOnStart() {
            this.teleportationParticles.SetActive(true);

            yield return this.StartCoroutine(this.FadeOutTransitionCylinder(this.sceneFadeDuration));

            yield return new WaitForSeconds(3.0f);
            this.teleportationParticles.SetActive(false);
        }

        /// <summary>
        /// Ienumerator that fades in the cylinder. Gives the player a feel of teleportation.
        /// This happens before the next level is loaded.
        /// </summary>
        private IEnumerator FadeInTransitionCylinder(float duration) {
            float elapsedTime = 0f;

            // Fade in cylinder (shortly darken everything around the player before scene transition)
            while (elapsedTime < duration) {
                float fadeInCylinderAlpha = Mathf.Lerp(0.0f, 1.0f, elapsedTime / duration);
                this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, fadeInCylinderAlpha);
                this.fadeToWhiteCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, 1.0f);
            this.fadeToWhiteCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);
        }

        /// <summary>
        /// Ienumerator that fades out the cylinder. Gives the player a feel of teleportation.
        /// This happens after the next level is loaded. Basically on the start of every level.
        /// </summary>
        private IEnumerator FadeOutTransitionCylinder(float duration) {
            float elapsedTime = 0f;

            // Fade out cylinder
            while (elapsedTime < duration) {
                float fadeInCylinderAlpha = Mathf.Lerp(1.0f, 0.0f, elapsedTime / duration);
                this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, fadeInCylinderAlpha);
                this.fadeToWhiteCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);

                elapsedTime += Time.deltaTime;
                yield return null;
            }

            this.fadeInCylinderMaterialColor = new Color(this.fadeInCylinderMaterialColor.r, this.fadeInCylinderMaterialColor.g, this.fadeInCylinderMaterialColor.b, 0.0f);
            this.fadeToWhiteCylinderMaterial.SetColor("_BaseColor", this.fadeInCylinderMaterialColor);
        }

        /// <summary>
        /// Get the index of the next level.
        /// </summary>
        /// <returns>The index of the next level.</returns>
        public int GetNextLevel() {
            return SceneManager.GetActiveScene().buildIndex + 1;
        }

        /// <summary>
        /// Get an audio clip from the sound effect dictionary.
        /// </summary>
        /// <param name="soundName">The name of the audio clip to get.</param>
        /// <returns>The audio clip.</returns>
        public AudioClip GetAudioClipFromDictionary(string soundName) {
            return this.soundEffectDictionary[soundName];
        }
    }
}
