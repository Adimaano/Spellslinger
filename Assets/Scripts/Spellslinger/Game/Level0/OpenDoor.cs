using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour {
    [SerializeField] private Animator animator;
    // Start is called before the first frame update
    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            animator.SetTrigger("openDoor");
        }
    }
}
