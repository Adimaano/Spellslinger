using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class QuickReset : MonoBehaviour
{
    new Vector3 checkpoint;
    void Awake()
    {
        // First Checkpoint is the player's level entrance position
        checkpoint = GameObject.FindWithTag("Player").transform.position;
    }

    public void ReachNextCheckpoint(GameObject checkpointObject)
    {
        checkpoint = checkpointObject.transform.position;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.tag == "Player")
        {
            other.transform.position = checkpoint;
        }
    }
}
