namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private GameObject hazard;
        private bool firstTime = true;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player" && firstTime)
            {
                hazard.GetComponent<QuickReset>().ReachNextCheckpoint(this.gameObject);
                this.GetComponent<AudioSource>().Play(0);
                firstTime = false;
            }
        }
    }
}
