using System;
using System.Collections;
using Spellslinger.Game.Control;
using Spellslinger.Misc;
using Unity.XR.CoreUtils;
using UnityEngine;

namespace Spellslinger.Game.AirLevel02
{
    [RequireComponent(typeof(Rigidbody))]
    [RequireComponent(typeof(ConstantForce))]
    public class BalloonController : MonoBehaviour
    {
        [SerializeField] private float airSpellEffect = 10f;

        // volume of the balloon in cubic meters
        [SerializeField] private float balloonVolume = 2800f;

        // temperature outside the balloon in celcius
        [SerializeField] private float outsideTemperatureC = 20f;
        [SerializeField] private float insideTemperatureC = 80f;

        [SerializeField] [ReadOnly] private Vector3 liftForce;
        [SerializeField] [ReadOnly] private float liftMass;
        [SerializeField] [ReadOnly] private Vector3 acceleration;

        private Rigidbody rb;
        private new ConstantForce constantForce;
        private SpellCasting spellCasting;
        private FurnaceController furnaceController;
        private KiteController kiteController;
        private Player player;

        private void Awake()
        {
            rb = GetComponent<Rigidbody>();
            constantForce = GetComponent<ConstantForce>();
            furnaceController = GetComponentInChildren<FurnaceController>();
            kiteController = GetComponentInChildren<KiteController>();
        }

        public void SetSpellCasting(SpellCasting spellCasting)
        {
            this.spellCasting = spellCasting;
            spellCasting.OnSpellCast += OnSpellCast;
        }

        public void SetPlayer(Player player)
        {
            this.player = player;
        }

        private void OnSpellCast(SpellCasting.Spell spell)
        {
            if (spell == SpellCasting.Spell.Air)
            {
                StartCoroutine(AirSpellCasting());
            }
        }

        private IEnumerator AirSpellCasting()
        {
            while (spellCasting.IsCasting)
            {
                if (!kiteController.InCurrent)
                {
                    yield return null;
                    continue;
                }

                var castObject = spellCasting.GetSpellcastingTarget(player.PreferredController);
                // get look vector of cast object
                var lookVector = castObject.transform.forward;
                // clamp vector so that horizontal movement is possible, but vertical movement is restricted
                lookVector = new Vector3(lookVector.x, lookVector.y * 0.1f, lookVector.z);
                // apply force from point of cast object in look direction to balloon
                rb.AddForceAtPosition(lookVector * airSpellEffect, castObject.transform.position,
                    ForceMode.Acceleration);
                yield return null;
            }
        }

        private float CelsiusToKelvin(float temp)
        {
            return temp + 273.15f;
        }

        private float AirDensity(float temp)
        {
            return 101325 / (287.058f * CelsiusToKelvin(temp));
        }

        private void FixedUpdate()
        {
            insideTemperatureC = furnaceController.TemperatureC;
            var densityDifference = AirDensity(outsideTemperatureC) - AirDensity(insideTemperatureC);
            // inverse gravity for calculation
            var gravity = -Physics.gravity;
            liftForce = gravity * Math.Max(balloonVolume * densityDifference, 0);
            liftMass = liftForce.magnitude / gravity.magnitude;

            // calculate acceleration
            acceleration = liftForce / rb.mass - gravity;
            if (acceleration.magnitude < 0.05f)
            {
                // apply gravity counter force to hover (calculate force to counter gravity)
                constantForce.force = gravity * rb.mass;
                return;
            }

            constantForce.force = liftForce;
        }
    }
}