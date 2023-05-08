using System.Collections;
using UnityEngine;

namespace Spellslinger.Game.Environment
{
    public class ParticleScript : MonoBehaviour {
        public float lifetime = 1.5f;

        void Awake() {
            StartCoroutine(DestroyParticle());
        }

        IEnumerator DestroyParticle() {
            yield return new WaitForSeconds(lifetime);
            Destroy(gameObject);
        }
    }
}
