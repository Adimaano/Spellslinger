using System.Collections;
using UnityEngine;

public class Rune : MonoBehaviour {
    private float timeAlive = 5.0f;
    
    void Start() {
        StartCoroutine("DestroyAfterTime", timeAlive);
    }

    IEnumerator DestroyAfterTime(float time) {
        yield return new WaitForSeconds(time);
        Destroy(gameObject);
    }
}

