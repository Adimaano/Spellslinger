
namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class UnstickPlayerFromDynamicArea : MonoBehaviour
    {
        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<StickToDynamicArea>().stick = false;
            }
        }
        private void OnTriggerExit(Collider other)
        {
            if (other.gameObject.tag == "Player")
            {
                other.GetComponent<StickToDynamicArea>().stick = true;
            }
        }
    }
}