namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Environment;
    using UnityEngine;

    public class Level01Manager : MonoBehaviour {
        [Header("First Puzzle | Fence")]
        [SerializeField] private Torch fenceTorch;
        [SerializeField] private Animator fenceAnimator;
        [SerializeField] private AudioClip fenceOpenSound;

        [Header("Second Puzzle | Mirrors")]
        [SerializeField] private Torch[] mirrorTorches;
        [SerializeField] private Transform playerHead;
        [SerializeField] private Transform playerOrigin;
        private bool isMirrorPuzzleSolved = false;
        // const with offset to mirror room
        private const float roomOffsetX = 29.5f;
        private const float roomOffsetY = 0.0f;
        private const float roomOffsetZ = -30f;


        [Header("Wizard Voice")]
        [SerializeField] private AudioSource wizardVoiceAudioSource;

        void Start() {
            this.fenceTorch.OnTorchLit = () => {
                this.StartCoroutine(this.OnFenceTorchLit());
            };
        }

        /// <summary>
        /// Called when the fence torch is lit. Lowers the fence.
        /// </summary>
        private IEnumerator OnFenceTorchLit() {
            this.fenceAnimator.SetBool("isFenceDown", true);
            this.fenceAnimator.gameObject.GetComponent<AudioSource>().PlayOneShot(this.fenceOpenSound);
            yield return null;
        }

        private void ResetPlayerPosition() {
            this.playerHead.position = new Vector3(this.playerHead.position.x - roomOffsetX, this.playerHead.position.y - roomOffsetY, this.playerHead.position.z - roomOffsetZ);
            this.playerOrigin.position = new Vector3(this.playerOrigin.position.x - roomOffsetX, this.playerOrigin.position.y - roomOffsetY, this.playerOrigin.position.z - roomOffsetZ);
        }

        private void OnTriggerEnter(Collider other) {
            Debug.Log("Triggerd");
            if (other.CompareTag("Player") && !this.isMirrorPuzzleSolved) {
                Debug.Log("MOVE PLAYER");
                this.ResetPlayerPosition();
            }
        }
    }
}