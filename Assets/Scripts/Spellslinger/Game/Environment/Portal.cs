using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spellslinger.Game;
using Spellslinger.Game.Environment;

namespace Spellslinger.Game.Environment
{
    public class Portal : MonoBehaviour {
        [SerializeField] private Material portalMaterial;
        [SerializeField] private Material portalMaterialDefault;
        public bool IsActive = false;

        private void OnTriggerEnter(Collider other) {
            if (IsActive && other.CompareTag("Player")) {
                // Set the final intensity value
                Color baseEmissionColor = this.portalMaterialDefault.GetColor("_EmissionColor");
                this.portalMaterial.SetColor("_EmissionColor", baseEmissionColor);
                GameManager.Instance.LoadLevel(2); // Temporarily hardcoded
            }
        }
    }
}