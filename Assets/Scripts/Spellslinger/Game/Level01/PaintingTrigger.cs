
namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class PaintingTrigger : MonoBehaviour {
        private Level01Manager levelManager;

        // one of three paintingnames as string
        [Header("Painting Info")]
        [Tooltip("Available painting names: oldPetunia, haplessPercival, lucius")]
        [SerializeField] private string paintingName = "oldPetunia";
        [SerializeField] private Transform paintingPosition;


        private void Start() {
            this.levelManager = GameObject.FindObjectOfType<Level01Manager>();

            if (this.levelManager == null) {
                Debug.LogError("Could not find Level01Manager in scene.");
            }
        }

        private void OnTriggerStay(Collider other) {
            if (other.CompareTag("Player")) {
                this.levelManager.TriggerPaintingSpeech(this.paintingName, paintingPosition);
            }
        }
    }
}
