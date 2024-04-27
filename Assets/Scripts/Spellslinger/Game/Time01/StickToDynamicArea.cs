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
        void OnTriggerEnter(Collider collision)
        {
            if (collision.gameObject.tag == "Player")
            {
               collision.GetComponent<Collider>().transform.SetParent(this.transform);
               Debug.Log("Player is now a child of " + this.gameObject.name);
            }
        }
        

         void OnTriggerExit(Collider collision)
        {
            if (collision.gameObject.tag == "Player")
            {
                collision.GetComponent<Collider>().transform.SetParent(XRparent);
            }
        }
    }
}