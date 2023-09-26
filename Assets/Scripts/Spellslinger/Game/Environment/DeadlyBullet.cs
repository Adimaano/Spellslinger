namespace Spellslinger.Game.Environment
{
    using Spellslinger.Game.Manager;
    using UnityEngine;

    public class DeadlyBullet : MonoBehaviour {
        private void OnCollisionEnter(Collision collision) {
            if (collision.gameObject.CompareTag("Player")) {
                GameManager.Instance.RestartLevel();
            }
        }
    }
}