namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class QuickReset : MonoBehaviour
    {   
        Vector3 checkpoint;
        void Start()
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
                other.gameObject.transform.position = checkpoint;
            }
        }
    }
}