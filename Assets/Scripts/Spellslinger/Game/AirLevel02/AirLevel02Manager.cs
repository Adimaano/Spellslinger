using System;
using Spellslinger.Game.Control;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Spellslinger.Game.AirLevel02
{
    public class AirLevel02Manager : MonoBehaviour
    {
        [SerializeField]
        private BalloonController balloonController;
        private GameObject xrRig;
        private Player player;
        private SpellCasting spellCasting;

        private void Awake()
        {
            xrRig = GameObject.Find("-- XR --");
            // find all TeleportationAreas
            var teleportationAreas = FindObjectsOfType<TeleportationArea>();
            foreach (var teleportationArea in teleportationAreas)
            {
                teleportationArea.teleporting.AddListener(OnTeleport);
            }
            
            spellCasting = xrRig.GetComponent<SpellCasting>();
            balloonController.SetSpellCasting(spellCasting);
            
            // get player
            player = xrRig.GetComponent<Player>();
            player.DisallowAirCastOnGround = true;
        }

        private void OnTeleport(TeleportingEventArgs args)
        {
            if (args.interactableObject.transform.CompareTag("Balloon"))
            {
                // reparent xr rig to parent of interactable object
                xrRig.transform.parent = args.interactableObject.transform.parent;
                // set spellcasting reference to balloon
                spellCasting.VelocityReference = balloonController.GetComponent<Rigidbody>();
            }
            else
            {
                // reparent xr rig to world
                xrRig.transform.parent = null;
                spellCasting.VelocityReference = null;
            }
        }
    }
}