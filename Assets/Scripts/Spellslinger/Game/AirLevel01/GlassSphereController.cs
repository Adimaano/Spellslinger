using System;
using Spellslinger.Game.Manager;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Spellslinger.Game.AirLevel01
{
    public class GlassSphereController : MonoBehaviour
    {
        private Rigidbody rigidbodyComponent;
        private AudioSource audioSourceComponent;

        private float waitForCollisionDetection = 0.1f;

        private void OnEnable()
        {
            this.waitForCollisionDetection = Time.time + 0.5f;
        }

        private void Awake()
        {
            this.rigidbodyComponent = this.gameObject.GetComponent<Rigidbody>();

            if (this.rigidbodyComponent == null)
            {
                this.rigidbodyComponent = this.gameObject.AddComponent<Rigidbody>();
            }

            this.audioSourceComponent = this.gameObject.GetComponent<AudioSource>();

            if (this.audioSourceComponent == null)
            {
                this.audioSourceComponent = this.gameObject.AddComponent<AudioSource>();
            }

            // set audio source to 3D
            this.audioSourceComponent.spatialBlend = 1.0f;

            this.waitForCollisionDetection += Time.time;
        }

        private void OnCollisionEnter(Collision other)
        {
            if (Time.time < this.waitForCollisionDetection)
            {
                return;
            }

            if (!other.gameObject.CompareTag("Player"))
            {
                int random = Random.Range(1, 10);
                this.PlaySound("Glass0" + random);
            }
        }

        private void PlaySound(string soundName, float volume = 0.85f)
        {
            AudioClip clip = GameManager.Instance.GetAudioClipFromDictionary(soundName);
            this.audioSourceComponent.PlayOneShot(clip, 0.85f);
        }
    }
}