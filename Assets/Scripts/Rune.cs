using System.Collections;
using UnityEngine;

public class Rune : MonoBehaviour {
    private float timeAlive = 5.0f;

    private void Start() {
        this.StartCoroutine("DestroyAfterTime", this.timeAlive);
    }

    private IEnumerator DestroyAfterTime(float time) {
        yield return new WaitForSeconds(time);
        Destroy(this.gameObject);
    }
}
