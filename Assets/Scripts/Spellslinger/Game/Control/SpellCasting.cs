using System;
using System.Collections;
using System.Collections.Generic;
using Spellslinger.Game.Spell;
using Spellslinger.Game.XR;
using UnityEngine;
using UnityEngine.Serialization;

namespace Spellslinger.Game.Control
{
    public class SpellCasting : MonoBehaviour {
        // Spells / Particle Effects
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject earthSpellPrefab;

        private GameObject spellCastingRight;
        private GameObject spellCastingLeft;
        private Vector3 spellCastingTarget = Vector3.zero;

        private bool isCasting = false;
        private GameObject castOnObject; // e.g. for reverting a spell (e.g. let earth pillar explode)
        private Spell currentSpell = Spell.None;
        private GameObject spellReticle;

        // enum with all possible Spells
        public enum Spell {
            Water,
            Fire,
            Earth,
            Air,
            Lightning,
            Time,
            None,
        }

        [Serializable]
        public struct SpellSettings {
            public Spell spell;
            public GameObject chargePrefab;
            public GameObject misslePrefab;
            public GameObject auraPrefab;
            public GameObject blastPrefab;
        }

        public SpellSettings[] spellSettings;
        private Dictionary<Spell, GameObject> spellChargeDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellMissleDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellAuraDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellBlastDictionary = new Dictionary<Spell, GameObject>();

        private void Start() {
            this.spellCastingRight = GameObject.Find("WandTipRight");
            this.spellCastingLeft = GameObject.Find("WandTipLeft");
            for (int i = 0; i < this.spellSettings.Length; i++) {
                this.spellChargeDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].chargePrefab);
                this.spellMissleDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].misslePrefab);
                this.spellAuraDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].auraPrefab);
                this.spellBlastDictionary.Add(this.spellSettings[i].spell, this.spellSettings[i].blastPrefab);
            }
        }

        private void Update() {
            if (this.spellCastingTarget != Vector3.zero) {
                if (this.spellReticle == null) {
                    // Instantiate the reticle prefab at the target position and rotate it to be horizontal
                    this.spellReticle = Instantiate(this.spellAuraDictionary[this.currentSpell], this.spellCastingTarget, Quaternion.identity);
                    this.spellReticle.transform.Rotate(new Vector3(-90, 0, 0));
                }
                this.spellReticle.transform.position = this.spellCastingTarget;
            } else if (this.spellReticle != null) {
                Destroy(this.spellReticle);
            }
        }

        public void ChargeSpell(SpellCasting.Spell spell, XRInputManager.Controller controller) {
            var target = controller == XRInputManager.Controller.Right ? this.spellCastingRight : this.spellCastingLeft;
            // Remove all children of the target
            foreach (Transform child in target.transform) {
                Destroy(child.gameObject);
            }

            this.currentSpell = spell;

            if (spell == Spell.None) {
                return;
            }

            // Instantiate the charge prefab
            GameObject charge = Instantiate(this.spellChargeDictionary[spell], target.transform);
            charge.transform.localPosition = Vector3.zero;
            charge.transform.localRotation = Quaternion.identity;
            // set size to 0.1
            charge.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
        }

        private void CastFireSpell1(GameObject spellOrigin) {
            GameObject fireball = Instantiate(this.fireballPrefab, spellOrigin.transform.position, Quaternion.identity);
            fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);

            FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
            spell.SpellDirection = spellOrigin.transform.forward;
        }

        private void CastEarthSpell() {
            GameObject earthSpell = Instantiate(this.earthSpellPrefab, this.spellCastingTarget, Quaternion.identity);
            
            StartCoroutine(this.EarthSpellCoroutine(earthSpell));
        }

        private IEnumerator EarthSpellCoroutine(GameObject earth) {
            this.isCasting = true;

            // grow earth gameobject in y direction for 2 seconds or until interrupted
            float time = 0;
            while (time < 1.5f && this.isCasting) {
                earth.transform.localScale += new Vector3(0, 0.5f, 0);
                time += Time.deltaTime;
                yield return null;
            }

            this.isCasting = false;
            earth.GetComponent<AudioSource>().Stop();
        }

        private void CastGenericSpell(GameObject origin, GameObject misslePrefab) {
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

        /// <summary>
        /// Casts a spell.
        /// </summary>
        /// <param name="spell">The spell to cast.</param>
        /// <param name="controller">The controller from which the spell is cast.</param>
        public void CastSpell(Spell spell, XRInputManager.Controller controller) {
            GameObject spellOrigin = controller == XRInputManager.Controller.Right
                ? this.spellCastingRight
                : this.spellCastingLeft;

            switch (spell) {
                case Spell.Fire:
                    this.CastFireSpell1(spellOrigin);
                    break;
                case Spell.Earth:
                    if (this.castOnObject != null) {
                        StartCoroutine(this.BlastGenericObject(spell));
                    } else if (this.spellCastingTarget == Vector3.zero) {
                        this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    } else {
                        this.CastEarthSpell();
                    }
                    break;
                default:
                    this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    break;
            }
        }

        private IEnumerator BlastGenericObject(Spell spell) {
            GameObject blastPillar = Instantiate(this.spellBlastDictionary[spell], this.castOnObject.transform.position, Quaternion.identity);
            blastPillar.transform.Rotate(new Vector3(-90, 0, 0));

            Destroy(this.castOnObject.transform.parent.gameObject);
            this.castOnObject = null;

            yield return new WaitForSeconds(1.5f);

            Destroy(blastPillar);
        }

        /// <summary>
        /// Sets the target for specific spells (e.g. earth spell).
        /// </summary>
        /// <param name="target">The target position for the spell. This is where the spell will be instantiated.</param>
        public void SetSpellCastingTarget(Vector3 target) {
            this.spellCastingTarget = target;
        }

        /// <summary>
        /// Interrupts the casting of a spell.
        /// </summary>
        public void InterruptCasting() {
            this.isCasting = false;
        }

        public void SetSpecialCasting(GameObject objectToCastMagicOn) {
            this.castOnObject = objectToCastMagicOn;
        }
    }
}