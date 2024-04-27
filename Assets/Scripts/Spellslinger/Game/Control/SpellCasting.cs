
namespace Spellslinger.Game.Control
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.Game.Spell;
    using Spellslinger.Game.XR;
    using UnityEngine;

    public class SpellCasting : MonoBehaviour
    {
        // Spells / Particle Effects
        [SerializeField] private GameObject fireballPrefab;
        [SerializeField] private GameObject earthSpellPrefab;
        [SerializeField] private GameObject airSpellPrefab;
        [SerializeField] private GameObject airSpellFromWandPrefab;
        public GameObject[] beamLineRendererPrefab;
        public GameObject[] beamStartPrefab;
        public GameObject[] beamEndPrefab;

        private int LightningBeam = 0;
        private int WaterBeam = 1;

        private GameObject beamStart;
        private GameObject beamEnd;
        private GameObject beam;
        private LineRenderer line;

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
        public enum Spell
        {
            Time = 0,
            Air = 1,
            Fire = 2,
            Earth = 3,
            Water = 4, 
            Lightning = 5,
            None = 6,
        }
        // enum with all possible Spells

        [Serializable]
        public struct SpellSettings
        {
            public Spell Spell;
            public GameObject ChargePrefab;
            public GameObject MisslePrefab;
            public GameObject AuraPrefab;
            public GameObject BlastPrefab;
        }

        public System.Action<Spell> OnSpellCast { get; internal set; }

        public bool IsCasting => this.isCasting;
        private Rigidbody velocityReference;

        public Rigidbody VelocityReference
        {
            get => this.velocityReference;
            set => this.velocityReference = value;
        }

        private void Start()
        {
            this.spellCastingRight = GameObject.Find("WandTipRight");
            this.spellCastingLeft = GameObject.Find("WandTipLeft");
            for (int i = 0; i < this.spellSettings.Length; i++)
            {
                this.spellChargeDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].ChargePrefab);
                this.spellMissleDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].MisslePrefab);
                this.spellAuraDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].AuraPrefab);
                this.spellBlastDictionary.Add(this.spellSettings[i].Spell, this.spellSettings[i].BlastPrefab);
            }
        }

        private void Update()
        {
            if (this.spellCastingTarget != Vector3.zero)
            {
                if (this.spellReticle == null && this.currentSpell != Spell.None)
                {
                    // Instantiate the reticle prefab at the target position and rotate it to be horizontal
                    this.spellReticle = Instantiate(this.spellAuraDictionary[this.currentSpell],
                        this.spellCastingTarget, Quaternion.identity);
                    this.spellReticle.transform.Rotate(new Vector3(-90, 0, 0));
                }

                this.spellReticle.transform.position = this.spellCastingTarget;
            }
            else if (this.spellReticle != null)
            {
                Destroy(this.spellReticle);
            }
        }
        
        public GameObject GetSpellcastingTarget(XRInputManager.Controller controller)
        {
            return controller == XRInputManager.Controller.Right ? this.spellCastingRight : this.spellCastingLeft;
        }

        /// <summary>
        /// Prepares a spell. Charges the wand with the spell.
        /// </summary>
        /// <param name="spell">The spell to charge.</param>
        /// <param name="controller">The controller/hand with the wand.</param>
        public void ChargeSpell(SpellCasting.Spell spell, XRInputManager.Controller controller)
        {
            var target = GetSpellcastingTarget(controller);

            // Remove all children of the target
            foreach (Transform child in target.transform)
            {
                // destroy all children NOT tagged "SpellManaged"
                if (!child.CompareTag("SpellManaged"))
                {
                    Destroy(child.gameObject);
                }
            }

            this.currentSpell = spell;

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

        /// <summary>
        /// Casts the Fire spell (projectile).
        /// </summary>
        /// <param name="spellOrigin">The origin of the spell.</param>
        private void CastFireSpell(GameObject spellOrigin)
        {
            GameObject fireball = Instantiate(this.fireballPrefab, spellOrigin.transform.position, Quaternion.identity);
            fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);
            // if velocity reference is set, add velocity of reference to fireball
            if (this.velocityReference != null)
            {
                fireball.GetComponent<Rigidbody>().velocity = this.velocityReference.velocity;
            }

            FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
            spell.SpellDirection = spellOrigin.transform.forward;
        }

        /// <summary>
        /// Instantiates an earth pillar and calls the coroutine for growing it.
        /// </summary>
        private void CastEarthSpell()
        {
            GameObject earthSpell = Instantiate(this.earthSpellPrefab, this.spellCastingTarget, Quaternion.identity);
            this.StartCoroutine(this.EarthSpellCoroutine(earthSpell));
        }

        /// <summary>
        /// Coroutine for casting the earth spell. Creates a pillar of earth at the target position.
        /// </summary>
        /// <param name="earth">The earth gameobject.</param>
        private IEnumerator EarthSpellCoroutine(GameObject earth)
        {
            this.isCasting = true;

            // grow earth gameobject in y direction for 2 seconds or until interrupted
            float time = 0;
            while (time < 1.5f && this.isCasting)
            {
                earth.transform.localScale += new Vector3(0, 1.25f, 0);
                time += Time.deltaTime;
                yield return null;
            }

            this.isCasting = false;
            earth.GetComponent<AudioSource>().Stop();
        }


        /// <summary>
        /// Triggers VFX and Audio for the time spell once on trigger.
        /// Instantiates the animation target and sets the playback mode.
        /// Calls the coroutine for controlling the target animation playback speed.
        /// </summary>
        private void CastTimeSpell(GameObject movingObject, GameObject wand)
        {
            bool refRight = (wand == this.spellCastingRight);
            Animator objectAnim = movingObject.GetComponent<Animator>();
            objectAnim.StartPlayback();
            movingObject.GetComponent<AudioSource>().Play(0);

            this.StartCoroutine(this.TimeSpellCoroutine(objectAnim, wand.transform.parent.transform.position,
                refRight));
        }


        /// <summary>
        /// Coroutine for casting the time spell. Controls the animation playback speed of target gameobject to simulatre "scrubbing" through time.
        /// </summary>
        /// <param name="objectAnim">Manipulatable animation of target gameobject</param>
        /// <param name="startPosOfWand">Position of wand at initial cast</param>
        private IEnumerator TimeSpellCoroutine(Animator objectAnim, Vector3 startPosOfWand, bool refRight)
        {
            //wand needs to either be a pointer or set as classmember which can be called
            this.isCasting = true;

            while (this.isCasting)
            {
                //For easier controls, we use the thumbstick for controlling the speed of the animation
                float delta = deltaControllerPos(startPosOfWand, refRight);
                //Debug.Log(delta);
                float offset = 0.3f;
                float gradient = 3f;
                if (delta > offset)
                {
                    objectAnim.speed = -(delta-offset) * gradient;
                }
                else if (delta < -offset)
                {
                    objectAnim.speed = -(delta+offset) * gradient;
                }
                else
                {
                    objectAnim.speed = 0.0F;
                }

                yield return null;
            }

            objectAnim.StopPlayback();
            objectAnim.speed = 1.0F;
            this.isCasting = false;
        }
        
        /// <summary>
        /// Calculates delta movement of controller in left-right direction.
        /// </summary>
        /// <param name="startPosOfWand">The start position of the focus.</param>
        public float deltaControllerPos(Vector3 startPosOfWand, bool refRight)
        {
            Vector3 distanceMoved = Vector3.zero;
            float delta = 0.0f;

            if (refRight)
            {
                distanceMoved = startPosOfWand - this.spellCastingRight.transform.position;
                // to the left.
            }
            else
            {
                distanceMoved = startPosOfWand - this.spellCastingLeft.transform.position;
                // to the right.
            }
            
            delta = distanceMoved.magnitude;
            Vector3 forwardVector = Camera.main.transform.forward;
            Vector3 crossProduct = Vector3.Cross(forwardVector, distanceMoved);

            // If the cross product's y-component is negative, it means the movement was to the right (in a left-handed coordinate system).
            bool movedRight = crossProduct.y < 0;
            Debug.Log("X prod: " + crossProduct);
            // Distance moved along the left or right side.
            if (movedRight)
            {
                delta = -delta;
                Debug.Log("Moved right! " + delta);
            }
            return delta;
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
        /// Casted spell shoots a lightning bolt from the wand to the target.
        /// </summary>
        /// <param name="origin">The origin of the spell.</param>
        /// <param name="misslePrefab">The missle prefab.</param>
        private void CastBeamSpell(GameObject wand, int currentBeam)
        {
            beamStart = Instantiate(beamStartPrefab[currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            beamStart.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            beamEnd = Instantiate(beamEndPrefab[currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            beamEnd.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            beam = Instantiate(beamLineRendererPrefab[currentBeam], new Vector3(0, 0, 0), Quaternion.identity) as GameObject;
            beam.transform.localScale = new Vector3(0.1f, 0.1f, 0.1f);
            line = beam.GetComponent<LineRenderer>();
            line.startWidth = 0.7f;
            line.endWidth = 0.7f;

            this.StartCoroutine(this.BeamSpellCoroutine(this.spellCastingRight.transform.position));

        }

        private IEnumerator BeamSpellCoroutine(Vector3 startPositionOfWand)
        {
            this.isCasting = true;
            RaycastHit hit;

            while (this.isCasting)
            {
                if (Physics.Raycast(startPositionOfWand, this.spellCastingRight.transform.forward, out hit))
                {
                    Vector3 tdir = hit.point - this.spellCastingRight.transform.position;
                    ShootBeamInDir(this.spellCastingRight.transform.position, tdir);
                }
                yield return null;
            }
            Destroy(beamStart);
            Destroy(beamEnd);
            Destroy(beam);
            isCasting = false;
        }
        
        private void ShootBeamInDir(Vector3 start, Vector3 dir)
        {
            #if UNITY_5_5_OR_NEWER
            line.positionCount = 2;
            #else
            line.SetVertexCount(2); 
            #endif
            line.SetPosition(0, start);
            beamStart.transform.position = start;

            Vector3 end = Vector3.zero;
            RaycastHit hit;
            if (Physics.Raycast(start, dir, out hit))
                end = hit.point - (dir.normalized * 2);
            else
                end = transform.position + (dir * 100);

            beamEnd.transform.position = end;
            line.SetPosition(1, end);

            beamStart.transform.LookAt(beamEnd.transform.position);
            beamEnd.transform.LookAt(beamStart.transform.position);

            float distance = Vector3.Distance(start, end);
            line.sharedMaterial.mainTextureScale = new Vector2(distance / 12, 1);
            line.sharedMaterial.mainTextureOffset -= new Vector2(Time.deltaTime * 4, 0);
        }

        /// <summary>
        /// Casted spell throws or lobs a projectile in the direction of the wand, which is affected by gravity.
        /// </summary>
        /// <param name="origin">The origin of the spell.</param>
        /// <param name="misslePrefab">The missle prefab.</param>
        private void CastBallisticProjectile(GameObject origin, GameObject misslePrefab)
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
            rigidbody.useGravity = true;
            rigidbody.mass = 0.01f;
            if (this.velocityReference != null)
            {
                rigidbody.velocity = this.velocityReference.velocity*3;
            }

            var collider = missle.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        /// <summary>
        /// Casted spell shoots a direct missile toward a direction.
        /// </summary>
        /// <param name="origin">The origin of the spell.</param>
        /// <param name="misslePrefab">The missle prefab.</param>
        private void CastShootProjectile(GameObject origin, GameObject misslePrefab)
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
            if (this.velocityReference != null)
            {
                rigidbody.velocity = this.velocityReference.velocity;
            }

            var collider = missle.AddComponent<SphereCollider>();
            collider.radius = 0.1f;
        }

        /// <summary>
        /// Casts a spell.
        /// </summary>
        /// <param name="spell">The spell to cast.</param>
        /// <param name="controller">The controller from which the spell is cast.</param>
        public void CastSpell(Spell spell, XRInputManager.Controller controller)
        {
            GameObject spellOrigin = controller == XRInputManager.Controller.Right
                ? this.spellCastingRight
                : this.spellCastingLeft;

            switch (spell)
            {
                case Spell.Fire:
                    this.CastFireSpell(spellOrigin);
                    break;
                case Spell.Earth:
                    if (this.castOnObject != null)
                    {
                        this.StartCoroutine(this.BlastGenericObject(spell));
                    }
                    else if (this.spellCastingTarget == Vector3.zero)
                    {
                        this.CastBallisticProjectile(spellOrigin, this.spellMissleDictionary[spell]);
                    }
                    else
                    {
                        this.CastEarthSpell();
                    }

                    break;
                case Spell.Time:
                    if (this.castOnObject != null)
                    {
                        this.CastTimeSpell(castOnObject, spellOrigin);
                    }
                    else
                    {
                        this.CastShootProjectile(spellOrigin, this.spellMissleDictionary[spell]);
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

                case Spell.Water:
                    CastBeamSpell(spellOrigin, WaterBeam);

                    break;
                
                case Spell.Lightning:
                    CastBeamSpell(spellOrigin, LightningBeam);
            
                    break;
                default:
                    this.CastShootProjectile(spellOrigin, this.spellMissleDictionary[spell]);
                    break;
            }

            this.OnSpellCast?.Invoke(spell);
        }

        /// <summary>
        /// Instantiates a blast prefab at the position of the object that will be destroyed.
        /// </summary>
        /// <param name="spell">The spell that is cast.</param>
        private IEnumerator BlastGenericObject(Spell spell)
        {
            GameObject blastPillar = Instantiate(this.spellBlastDictionary[spell], this.castOnObject.transform.position,
                Quaternion.identity);
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
        public void SetSpellCastingTarget(Vector3 target)
        {
            this.spellCastingTarget = target;
        }

        /// <summary>
        /// Interrupts the casting of a spell.
        /// </summary>
        public void InterruptCasting()
        {
            this.isCasting = false;
        }

        /// <summary>
        /// Sets the object on which the spell will be cast.
        /// </summary>
        /// <param name="objectToCastMagicOn">The object on which the spell will be cast.</param>
        public void SetSpecialCasting(GameObject objectToCastMagicOn)
        {
            this.castOnObject = objectToCastMagicOn;
        }

    }
}