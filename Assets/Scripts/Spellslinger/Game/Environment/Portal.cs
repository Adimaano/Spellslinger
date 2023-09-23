using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spellslinger.Game;
using Spellslinger.Game.Environment;

public class Portal : MonoBehaviour {
    [SerializeField] private Material portalMaterial;
    public bool IsActive = false;


    private void OnTriggerEnter(Collider other) {
        if (IsActive && other.CompareTag("Player")) {
            // Set the final intensity value
            Color baseEmissionColor = this.portalMaterial.GetColor("_EmissionColor");
            Color finalEmissionColor = baseEmissionColor * -20.0f;
            this.portalMaterial.SetColor("_EmissionColor", finalEmissionColor);
            GameManager.Instance.LoadLevel(2);
        }
    }
}
