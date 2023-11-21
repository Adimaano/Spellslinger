namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StickToDynamicArea : MonoBehaviour
    {
        public bool stick = true;
        void OnCollisionStay(Collision collision)
        {
            if (collision.gameObject.tag == "TimeTarget" && stick)
            {
                this.transform.position = collision.transform.position;
            }
        }
    }
}