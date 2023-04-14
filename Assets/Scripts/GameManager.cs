using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour {
    public static GameManager Instance;

    private AudioSource soundEffectSource;
    private AudioSource musicSource;

    [Header("Audio")]
    [SerializeField] private AudioClip[] soundEffects;

    // A dictionary to map sound effect names to audio clips
    private Dictionary<string, AudioClip> soundEffectDictionary;

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
        this.soundEffectSource = this.transform.Find("Sounds").GetComponent<AudioSource>();
        this.musicSource = this.transform.Find("Music").GetComponent<AudioSource>();

        // Initialize the sound effect dictionary
        this.soundEffectDictionary = new Dictionary<string, AudioClip>();
        for (int i = 0; i < this.soundEffects.Length; i++) {
            this.soundEffectDictionary.Add(this.soundEffects[i].name, this.soundEffects[i]);
        }
    }

    public void PlaySound(string soundName, float volume = 1.0f) {
        AudioClip clip = this.soundEffectDictionary[soundName];
        this.soundEffectSource.PlayOneShot(clip, volume);
    }
}
