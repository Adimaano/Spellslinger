
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
        void OnCollisionEnter(Collision other)
        {
            if (other.gameObject.tag == "Player")
            {
               other.transform.SetParent(XRparent);
            }
        }
    }
}