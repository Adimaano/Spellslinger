
namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UnstickPlayerFromDynamicArea : MonoBehaviour
    {
        private Transform XRparent;
        private void Start()
        {
            XRparent = GameObject.Find("-- XR --").transform;
        }
        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Player")
            {
               collision.transform.SetParent(XRparent);
            }
        }
    }
}