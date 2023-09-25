using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OpenDoor : MonoBehaviour {
    [SerializeField] private Animator animator;

    private void OnTriggerEnter(Collider other) {
        if (other.CompareTag("Player")) {
            this.animator.SetTrigger("openDoor");
        }
    }
}
