using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorAnimator : MonoBehaviour
{
    [SerializeField] private Animator animatorL, animatorR;
    [SerializeField] private bool open = false;
    [SerializeField] private bool close = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            if(open)
            {
                animatorR.Play("SwingDoorR", 0, 0.0f);
                animatorL.Play("SwingDoorL", 0, 0.0f);
            }
            if(close)
            {
                animatorL.Play("SwingDoorLback", 0, 0.0f);
                animatorR.Play("SwingDoorRback", 0, 0.0f);
            }
        }
    }
}
