using System;
using System.Collections.Generic;
using Spellslinger.Game.Spell;
using Spellslinger.Game.XR;
using UnityEngine;
using UnityEngine.Serialization;

namespace Spellslinger.Game.Control
{
    public class SpellCasting : MonoBehaviour
    {
        // Spells / Particle Effects
        [SerializeField] private GameObject fireballPrefab;

        private GameObject spellCastingRight;
        private GameObject spellCastingLeft;

        // enum with all possible Spells
        public enum Spell
        {
            Water,
            Fire,
            Earth,
            Air,
            Lightning,
            Time,
            None,
        }

        [Serializable]
        public struct SpellSettings
        {
            public Spell spell;
            public GameObject chargePrefab;
            public GameObject misslePrefab;
        }

        public SpellSettings[] spellSettings;
        private Dictionary<Spell, GameObject> spellChargeDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellMissleDictionary = new Dictionary<Spell, GameObject>();

        private void Start()
        {
            this.spellCastingRight = GameObject.Find("WandTipRight");
            this.spellCastingLeft = GameObject.Find("WandTipLeft");
            for (int i = 0; i < this.spellSettings.Length; i++)
            {
                this.spellChargeDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].chargePrefab);
                this.spellMissleDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].misslePrefab);
            }
        }

        public void ChargeSpell(SpellCasting.Spell spell, XRInputManager.Controller controller)
        {
            var target = controller == XRInputManager.Controller.Right ? this.spellCastingRight : this.spellCastingLeft;
            // Remove all children of the target
            foreach (Transform child in target.transform)
            {
                Destroy(child.gameObject);
            }

            if (spell == Spell.None)
            {
                return;
            }

            // Instantiate the charge prefab
            GameObject charge = Instantiate(this.spellChargeDictionary[spell], target.transform);
            charge.transform.localPosition = Vector3.zero;
            charge.transform.localRotation = Quaternion.identity;
            // set size to 0.1
            charge.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        private void CastFireSpell1(GameObject spellOrigin)
        {
            GameObject fireball = Instantiate(this.fireballPrefab, spellOrigin.transform.position, Quaternion.identity);
            fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);

            FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
            spell.SpellDirection = spellOrigin.transform.forward;
        }

        private void CastGenericSpell(GameObject origin, GameObject misslePrefab)
        {
            var missle = Instantiate(misslePrefab, origin.transform.position, Quaternion.identity);
            // scale to 0.7
            missle.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            missle.transform.LookAt(origin.transform.parent.transform.position);
            
            // add GenericSpell script to missle
            var spell = missle.AddComponent<GenericSpell>();
            spell.SpellDirection = origin.transform.forward;
            
            // add rigidbody with no gravity and sphere collider to missle
            var rigidbody = missle.AddComponent<Rigidbody>();
            rigidbody.useGravity = false;
            rigidbody.mass = 0.1f;
            
            var collider = missle.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        public void CastSpell(Spell spell, XRInputManager.Controller controller)
        {
            GameObject spellOrigin = controller == XRInputManager.Controller.Right
                ? this.spellCastingRight
                : this.spellCastingLeft;

            switch (spell)
            {
                case Spell.Fire:
                    this.CastFireSpell1(spellOrigin);
                    break;
                default:
                    this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    break;
            }
        }
    }
}