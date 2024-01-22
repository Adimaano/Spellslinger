using System;
using Spellslinger.Game.Control;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

namespace Spellslinger.Game.AirLevel02
{
    public class AirLevel02Manager : MonoBehaviour
    {
        [SerializeField] private BalloonController balloonController;

        [SerializeField] private GameObject[] rockPrefabs;
        [SerializeField] private Transform rockSpawnCenter;
        [SerializeField] private Vector3 rockSpawnMaxOffset = new Vector3(100f, 100f, 100f);
        [SerializeField] private int rockSpawnCount = 100;

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
            
            balloonController.SetPlayer(player);
        }

        private void Start()
        {
            // spawn rocks randomly within the offset boundaries, centered on the rockSpawnCenter
            for (int i = 0; i < rockSpawnCount; i++)
            {
                // empty gameobject as parent for the rock, with rockSpawnCenter as parent
                var rockParent = new GameObject("Rock").transform;
                rockParent.parent = rockSpawnCenter;
                var rockPrefab = rockPrefabs[UnityEngine.Random.Range(0, rockPrefabs.Length)];
                var rock = Instantiate(rockPrefab, rockParent, true);
                rockParent.position = rockSpawnCenter.position + new Vector3(
                    UnityEngine.Random.Range(-rockSpawnMaxOffset.x, rockSpawnMaxOffset.x),
                    UnityEngine.Random.Range(-rockSpawnMaxOffset.y, rockSpawnMaxOffset.y),
                    UnityEngine.Random.Range(-rockSpawnMaxOffset.z, rockSpawnMaxOffset.z)
                );
                // add Outline Script
                var outline = rock.AddComponent<Outline>();
                outline.enabled = false;
                // tag with "StonePlatform" so that the player can destroy it with the earth spell
                rock.tag = "StonePlatform";
            }
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