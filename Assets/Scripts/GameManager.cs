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

    void Awake() {
        if (Instance == null) {
            Instance = this;
        } else {
            Destroy(gameObject);
        }

        DontDestroyOnLoad(gameObject);
    }

    // Start is called before the first frame update
    void Start() {
        soundEffectSource = transform.Find("Sounds").GetComponent<AudioSource>();
        musicSource = transform.Find("Music").GetComponent<AudioSource>();

        // Initialize the sound effect dictionary
        soundEffectDictionary = new Dictionary<string, AudioClip>();
        for (int i = 0; i < soundEffects.Length; i++) {
            soundEffectDictionary.Add(soundEffects[i].name, soundEffects[i]);
        }
    }


    public void PlaySound(string soundName, float volume=1.0f) {
        AudioClip clip = soundEffectDictionary[soundName];
        soundEffectSource.PlayOneShot(clip, volume);
    }
}
