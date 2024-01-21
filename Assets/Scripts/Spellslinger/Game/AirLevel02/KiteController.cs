using System;
using UnityEngine;

namespace Spellslinger.Game.AirLevel02
{
    public class KiteController : MonoBehaviour
    {
        
        private bool inCurrent = false;
        public bool InCurrent { get => inCurrent; private set => inCurrent = value; }
        
        private void OnTriggerExit(Collider other)
        {
            if (other.CompareTag("AirCurrent"))
            {
                inCurrent = false;
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.CompareTag("AirCurrent"))
            {
                inCurrent = true;
            }
        }
    }
}