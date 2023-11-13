using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StickToDynamicArea : MonoBehaviour
{
    private bool stick = true;
    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            stick = false;
        }
    }
    private void OnTriggerExit(Collider other)
    {
        if (other.gameObject.tag == "Obstacle")
        {
            stick = true;
        }
    }
    private void OnTriggerStay(Collider other)
    {
        if (other.gameObject.tag == "Player" && stick)
        {
            other.transform.position = this.transform.position;
        }
    }
}
