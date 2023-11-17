namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StickToDynamicArea : MonoBehaviour
    {
        public bool stick = true;
        private void OnTriggerStay(Collider other)
        {
            if (other.gameObject.tag == "TimeTarget" && stick)
            {
                this.transform.position = other.transform.position;
            }
        }
    }
}