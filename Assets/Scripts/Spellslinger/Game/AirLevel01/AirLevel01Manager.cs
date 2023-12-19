using System;
using Spellslinger.Game.Control;
using Spellslinger.Game.Environment;
using UnityEngine;

namespace Spellslinger.Game.AirLevel01
{
    public class AirLevel01Manager : MonoBehaviour
    {
        [SerializeField] private GameObject crystalBall;
        [SerializeField] private Transform crystalBallStartTransform;
        [SerializeField] private Portal portal;

        [SerializeField] private PedestalController[] pedestals = new PedestalController[3];
        [SerializeField] private PedestalController finalPedestal;

        private void Start()
        {
            GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Air);
            
            foreach (var ped in pedestals)
            {
                ped.IsActivated.AddListener(Activated);
            }
            
            finalPedestal.IsActivated.AddListener(Activated);
        }

        private void Activated()
        {
            // Check if all pedestals are activated
            foreach (var ped in pedestals)
            {
                if (!ped.Active)
                {
                    return;
                }
            }
            if (!finalPedestal.Active)
            {
                return;
            }

            portal.IsActive = true;
            // enable first child of portal
            portal.transform.GetChild(0).gameObject.SetActive(true);
        }

        private void FixedUpdate()
        {
            if (finalPedestal.Active)
            {
                return;
            }

            // Check if crystal ball is below 30 units from the start position y
            if (crystalBall.transform.position.y < crystalBallStartTransform.position.y - 30)
            {
                // Reset crystal ball position
                crystalBall.transform.position = crystalBallStartTransform.position;
                // Reset crystal ball velocity
                crystalBall.GetComponent<Rigidbody>().velocity = Vector3.zero;
                // Reset crystal ball angular velocity
                crystalBall.GetComponent<Rigidbody>().angularVelocity = Vector3.zero;
            }
        }
    }
}