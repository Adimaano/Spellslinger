namespace Spellslinger.Game.Environment
{
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class DeadlyBullet : MonoBehaviour {
        private void OnTriggerEnter(Collider other) {
            if (other.gameObject.CompareTag("Player")) {
                GameManager.Instance.RestartLevel();
            }
        }
    }
}