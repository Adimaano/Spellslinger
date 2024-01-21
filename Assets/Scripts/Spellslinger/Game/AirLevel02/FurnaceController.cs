using System;
using Spellslinger.Game.Spell;
using TMPro;
using UnityEngine;

namespace Spellslinger.Game.AirLevel02
{
    public class FurnaceController: MonoBehaviour
    {
        
        [SerializeField] private TextMeshPro temperatureText;
        [SerializeField] private float temperatureDecayRate = 0.02f;
        [SerializeField] private float temperatureIncreaseRate = 1.5f;
        [SerializeField] private float temperatureDecreaseRate = 1.5f;
        [SerializeField] private float temperatureMax = 100f;
        [SerializeField] private float temperatureMin = 20f;
        public float TemperatureC { get => temperatureC; private set => temperatureC = value; }
        
        private float temperatureC = 80f;
        
        private void Update()
        {
            temperatureText.text = $"{temperatureC:0.00}°C";
        }

        private void FixedUpdate()
        {
            temperatureC = Mathf.Clamp(temperatureC - temperatureDecayRate * Time.fixedDeltaTime, temperatureMin, temperatureMax);
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.gameObject.tag == "Fire") {
                temperatureC = Mathf.Clamp(temperatureC + temperatureIncreaseRate, temperatureMin, temperatureMax);
                // get FireBallSpell
                FireBallSpell fireBallSpell = other.gameObject.GetComponent<FireBallSpell>();
                fireBallSpell.Explode();
            } else if (other.gameObject.tag == "Water") {
                temperatureC = Mathf.Clamp(temperatureC - temperatureDecreaseRate, temperatureMin, temperatureMax);
                Destroy(other.gameObject);
            }
        }
    }
}