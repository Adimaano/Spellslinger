namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class Checkpoint : MonoBehaviour
    {
        [SerializeField] private GameObject hazard;
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                hazard.GetComponent<QuickReset>().ReachNextCheckpoint(this.gameObject);
            }
        }
    }
}
