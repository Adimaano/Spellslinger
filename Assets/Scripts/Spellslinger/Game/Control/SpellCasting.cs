namespace Spellslinger.Game.Control
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Spell;
    using Spellslinger.Game.XR;
    using UnityEngine;

    public class SpellCasting : MonoBehaviour {
        // Spells / Particle Effects
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject earthSpellPrefab;
        [SerializeField] private GameObject airSpellPrefab;
        [SerializeField] private GameObject airSpellFromWandPrefab;

        private GameObject spellCastingRight;
        private GameObject spellCastingLeft;
        private Vector3 spellCastingTarget = Vector3.zero;
        private Vector3 wandPosition = Vector3.zero;

        private bool isCasting = false;
        private GameObject castOnObject; // e.g. for reverting a spell (e.g. let earth pillar explode)
        private Spell currentSpell = Spell.None;
        private GameObject spellReticle;

        [SerializeField] private SpellSettings[] spellSettings;

        private Dictionary<Spell, GameObject> spellChargeDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellMissleDictionary = new Dictionary<Spell, GameObject>();  
        private Dictionary<Spell, GameObject> spellAuraDictionary = new Dictionary<Spell, GameObject>();
        private Dictionary<Spell, GameObject> spellBlastDictionary = new Dictionary<Spell, GameObject>();

        // enum with all possible Spells
        public enum Spell {
            Time = 0,
            Air = 1,
            Fire = 2,
            Earth = 3,
            Water = 4,
            Lightning = 5,
            None = 6,
        }

        [Serializable]
        public struct SpellSettings {
            public Spell Spell;
            public GameObject ChargePrefab;
            public GameObject MisslePrefab;
            public GameObject AuraPrefab;
            public GameObject BlastPrefab;
        }

        public System.Action<Spell> OnSpellCast { get; internal set; }

        private void Start() {
            this.spellCastingRight = GameObject.Find("WandTipRight");
            this.spellCastingLeft = GameObject.Find("WandTipLeft");
            for (int i = 0; i < this.spellSettings.Length; i++) {
                this.spellChargeDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].ChargePrefab);
                this.spellMissleDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].MisslePrefab);
                this.spellAuraDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].AuraPrefab);
                this.spellBlastDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].BlastPrefab);
            }
        }

        private void Update() {
            if (this.spellCastingTarget != Vector3.zero) {
                if (this.spellReticle == null && this.currentSpell != Spell.None) {
                    // Instantiate the reticle prefab at the target position and rotate it to be horizontal
                    this.spellReticle = Instantiate(this.spellAuraDictionary[this.currentSpell], this.spellCastingTarget, Quaternion.identity);
                    this.spellReticle.transform.Rotate(new Vector3(-90, 0, 0));
                }

                this.spellReticle.transform.position = this.spellCastingTarget;
            } else if (this.spellReticle != null) {
                Destroy(this.spellReticle);
            }
        }

        /// <summary>
        /// Prepares a spell. Charges the wand with the spell.
        /// </summary>
        /// <param name="spell">The spell to charge.</param>
        /// <param name="controller">The controller/hand with the wand.</param>
        public void ChargeSpell(SpellCasting.Spell spell, XRInputManager.Controller controller) {
            var target = controller == XRInputManager.Controller.Right ? this.spellCastingRight : this.spellCastingLeft;

            // Remove all children of the target
            foreach (Transform child in target.transform) {
                // destroy all children NOT tagged "SpellManaged"
                if (!child.CompareTag("SpellManaged")) {
                    Destroy(child.gameObject);
                }
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

        /// <summary>
        /// Casts the Fire spell (projectile).
        /// </summary>
        /// <param name="spellOrigin">The origin of the spell.</param>
        private void CastFireSpell(GameObject spellOrigin) {
            GameObject fireball = Instantiate(this.fireballPrefab, spellOrigin.transform.position, Quaternion.identity);
            fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);

            FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
            spell.SpellDirection = spellOrigin.transform.forward;
        }

        /// <summary>
        /// Instantiates an earth pillar and calls the coroutine for growing it.
        /// </summary>
        private void CastEarthSpell() {
            GameObject earthSpell = Instantiate(this.earthSpellPrefab, this.spellCastingTarget, Quaternion.identity);
            this.StartCoroutine(this.EarthSpellCoroutine(earthSpell));
        }


        /// <summary>
        /// Triggers VFX and Audio for the time spell once on trigger.
        /// Instantiates the animation target and sets the playback mode.
        /// Calls the coroutine for controlling the target animation playback speed.
        /// </summary>
        private void CastTimeSpell(GameObject movingObject, GameObject wand) {
            bool refRight = (wand == this.spellCastingRight);
            Animator objectAnim = movingObject.GetComponent<Animator>();
            objectAnim.StartPlayback();
            movingObject.GetComponent<AudioSource>().Play(0);

            
            this.StartCoroutine(this.TimeSpellCoroutine(objectAnim, wand.transform.parent.transform.position, refRight));
        }

        /// <summary>
        /// Coroutine for casting the earth spell. Creates a pillar of earth at the target position.
        /// </summary>
        /// <param name="earth">The earth gameobject.</param>
        private IEnumerator EarthSpellCoroutine(GameObject earth) {
            this.isCasting = true;

            // grow earth gameobject in y direction for 2 seconds or until interrupted
            float time = 0;
            while (time < 1.5f && this.isCasting) {
                earth.transform.localScale += new Vector3(0, 1.25f, 0);
                time += Time.deltaTime;
                yield return null;
            }

            this.isCasting = false;
            earth.GetComponent<AudioSource>().Stop();
        }

        /// <summary>
        /// Cast the air spell, creating a air current at the target position.
        /// </summary>#
        private void CastAirSpell(GameObject wand)
        {
            GameObject airSpell = Instantiate(this.airSpellPrefab, this.spellCastingTarget, Quaternion.identity);
            this.StartCoroutine(this.AirSpellCoroutine(airSpell, wand.transform.parent.transform.position));
        }
        
        private void CastAirSpellFromWand(GameObject wand)
        {
            var airSpell = Instantiate(this.airSpellFromWandPrefab, wand.transform);
            airSpell.transform.localPosition = Vector3.zero;
            // rotate 90 degrees around x axis
            airSpell.transform.localRotation = Quaternion.Euler(90, 0, 0);
            // set "SpellManaged" tag to air current
            airSpell.tag = "SpellManaged";
            this.StartCoroutine(this.AirSpellFromWandCoroutine(airSpell));
        }
        
        private IEnumerator AirSpellFromWandCoroutine(GameObject airCurrent)
        {
            this.isCasting = true;
            
            // Get the AirCurrentSpell from the air current
            var airCurrentSpell = airCurrent.GetComponent<AirCurrentSpell>();
            airCurrentSpell.UpdateCurrent(Vector3.up, 0.3f, true);
            airCurrentSpell.StartCurrent();

            while (this.isCasting)
            {
                yield return null;
            }
            isCasting = false;
            // Destroy air current
            Destroy(airCurrent);
        }
        
        private IEnumerator AirSpellCoroutine(GameObject airCurrent, Vector3 startPositionOfWand)
        {
            this.isCasting = true;
            
            // Get the AirCurrentSpell from the air current
            var airCurrentSpell = airCurrent.GetComponent<AirCurrentSpell>();

            while (this.isCasting)
            {
                // Get complete vector delta of wand movement
                Vector3 delta = this.spellCastingRight.transform.parent.transform.position - startPositionOfWand;
                // Scale magnitude a little to make controlling easier
                airCurrentSpell.UpdateCurrent(delta.normalized, delta.magnitude * 1.5f);
                yield return null;
            }
            isCasting = false;
            airCurrentSpell.StartCurrent();
        }

        /// <summary>
        /// Coroutine for casting the time spell. Controls the animation playback speed of target gameobject to simulatre "scrubbing" through time.
        /// </summary>
        /// <param name="objectAnim">Manipulatable animation of target gameobject</param>
        /// <param name="startPosOfWand">Position of wand at initial cast</param>
        private IEnumerator TimeSpellCoroutine(Animator objectAnim, Vector3 startPosOfWand, bool refRight) { //wand needs to either be a pointer or set as classmember which can be called
            this.isCasting = true;
            
            while(this.isCasting) {
                float delta = deltaControllerPos(startPosOfWand, refRight);
                float offset = 0.075f;
                if(delta > offset) {
                    if(delta*4 > 1.0f)
                    {
                        objectAnim.speed = -1.0F;
                    }
                    else
                    {
                        objectAnim.speed = -delta*4;
                    }
                } else if(delta < -(offset/2)) {
                    if(delta*4 < -1.0f)
                    {
                        objectAnim.speed = 1.0F;
                    }
                    else
                    {
                        objectAnim.speed = -delta*4;
                    }
                } else {
                    objectAnim.speed = 0.0F;
                }
                yield return null;
            }
            objectAnim.StopPlayback();
            objectAnim.speed = 1.0F;
            this.isCasting = false;
        }

        /// <summary>
        /// Casts a generic spell.
        /// </summary>
        /// <param name="origin">The origin of the spell.</param>
        /// <param name="misslePrefab">The missle prefab.</param>
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
                    this.CastFireSpell(spellOrigin);
                    break;
                case Spell.Earth:
                    if (this.castOnObject != null) {
                        this.StartCoroutine(this.BlastGenericObject(spell));
                    } else if (this.spellCastingTarget == Vector3.zero) {
                        this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    } else {
                        this.CastEarthSpell();
                    }

                    break;
                case Spell.Time:
                    if (this.castOnObject != null){
                        this.CastTimeSpell(castOnObject, spellOrigin);
                    } else {
                        this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    }

                    break;
                case Spell.Air:
                    // Create air current at target position
                    if (this.spellCastingTarget == Vector3.zero)
                    {
                        CastAirSpellFromWand(spellOrigin);
                    }
                    // Start casting air current from wand
                    else
                    {
                        CastAirSpell(spellOrigin);
                    }
                    break;
                default:
                    this.CastGenericSpell(spellOrigin, this.spellMissleDictionary[spell]);
                    break;
            }

            this.OnSpellCast?.Invoke(spell);
        }

        /// <summary>
        /// Instantiates a blast prefab at the position of the object that will be destroyed.
        /// </summary>
        /// <param name="spell">The spell that is cast.</param>
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

        /// <summary>
        /// Sets the object on which the spell will be cast.
        /// </summary>
        /// <param name="objectToCastMagicOn">The object on which the spell will be cast.</param>
        public void SetSpecialCasting(GameObject objectToCastMagicOn) {
            this.castOnObject = objectToCastMagicOn;
        }

        /// <summary>
        /// Calculates delta movement of controller in x direction.
        /// </summary>
        /// <param name="startPosOfWand">The target position for the spell. This is where the spell will be instantiated.</param>
        public float deltaControllerPos(Vector3 startPosOfWand, bool refRight) {
            float delta = 0.0f;

            if (refRight) {
                delta = Camera.main.WorldToViewportPoint(startPosOfWand).x - Camera.main.WorldToViewportPoint(this.spellCastingRight.transform.parent.transform.position).x;
            } else {
                delta = Camera.main.WorldToViewportPoint(startPosOfWand).x - Camera.main.WorldToViewportPoint(this.spellCastingLeft.transform.parent.transform.position).x;
            }
            Debug.Log(Camera.main.WorldToViewportPoint(startPosOfWand));
            Debug.Log(Camera.main.WorldToViewportPoint(this.spellCastingRight.transform.parent.transform.position));
            Debug.Log(delta);
            return delta;
        }
    }
}