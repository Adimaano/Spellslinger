using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    [SerializeField] private Animator animatorL;
    [SerializeField] private Animator animatorR;
    [SerializeField] private bool open = false;
    [SerializeField] private bool close = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if (this.open)
            {
                this.animatorR.Play("SwingDoorR", 0, 0.0f);
                this.animatorL.Play("SwingDoorL", 0, 0.0f);
            }

            if (this.close)
            {
                this.animatorL.Play("SwingDoorLback", 0, 0.0f);
                this.animatorR.Play("SwingDoorRback", 0, 0.0f);
            }
        }
    }
}
