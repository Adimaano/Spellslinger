using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
