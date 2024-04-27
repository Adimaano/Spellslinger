namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class StickToDynamicArea : MonoBehaviour
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
               other.transform.SetParent(this.transform);
               Debug.Log("Player is now a child of " + this.gameObject.name);
            }
        }
        
        void OnCollisionExit(Collision other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.transform.SetParent(XRparent);
            }
        }
    }
}