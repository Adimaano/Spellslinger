namespace Spellslinger.Game.Environment
{
    using System.Collections;
    using UnityEngine;

    public class ParticleScript : MonoBehaviour {
        [SerializeField] private float lifetime = 1.5f;

        private void Awake() {
            this.StartCoroutine(this.DestroyParticle());
        }

        private IEnumerator DestroyParticle() {
            yield return new WaitForSeconds(this.lifetime);
            Destroy(this.gameObject);
        }
    }
}
