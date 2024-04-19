namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class ReleaseChandelier : MonoBehaviour
    {
        private GameObject chandelier;
        void Start()
        {
            chandelier = this.transform.parent.gameObject;
        }

        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Fire")
            {
                chandelier.GetComponent<Animator>().enabled = true;
            }
        }
    }
}