namespace Spellslinger.Game.Control
{
    using System.Collections;
    using System.Collections.Generic;
    using Spellslinger.AI;
    using Spellslinger.Game.Manager;
    using Spellslinger.Game.XR;
    using UnityEngine;

    public class Player : MonoBehaviour
    {
        private XRInputManager input;
        private Draw drawScript;
        private ModelRunner modelRunner;
        private SpellCasting spellCasting;

        private SpriteRenderer runeSpriteRenderer;

        private SpellCasting.Spell currentSpell = SpellCasting.Spell.None;
        private List<SpellCasting.Spell> availableSpells;

        // selected/highlighted gameobject
        private GameObject lastSelectedObject;

        // Rune Sprites
        [Header("Rune Sprites")] [SerializeField]
        private Sprite waterRune;

        [SerializeField] private Sprite fireRune;
        [SerializeField] private Sprite earthRune;
        [SerializeField] private Sprite airRune;
        [SerializeField] private Sprite lightningRune;
        [SerializeField] private Sprite timeRune;

        [Header("XR Objects for recentring")] [SerializeField]
        private Transform head;

        [SerializeField] private Transform origin;
        private GameObject playerXR;
        private Vector3 spawnPositionOrigin;
        private Quaternion spawnRotationOrigin;
        private Vector3 spawnPositionHead;
        private Quaternion spawnRotationHead;

        // Manual overrides for spells
        // If casting the air spell on the ground is allowed
        public bool DisallowAirCastOnGround { get; set; }

        public XRInputManager.Controller PreferredController { get; set; }

        private void Start()
        {
            // find dependencies in scene
            this.playerXR = GameObject.Find("-- XR --");
            this.input = this.playerXR.GetComponent<XRInputManager>();
            this.drawScript = this.playerXR.GetComponent<Draw>();
            this.modelRunner = this.playerXR.GetComponent<ModelRunner>();
            this.spellCasting = this.playerXR.GetComponent<SpellCasting>();
            this.runeSpriteRenderer =
                GameObject.Find("HUD-Canvas").transform.Find("Rune").GetComponent<SpriteRenderer>();

            this.spawnPositionOrigin = this.origin.transform.position;
            this.spawnRotationOrigin = this.origin.transform.rotation;
            this.spawnPositionHead = this.head.transform.position;
            this.spawnRotationHead = this.head.transform.rotation;

            // initialize eventlisteners
            this.drawScript.OnDrawFinished += this.ChargeSpell;
            this.input.OnControllerTrigger += this.DrawRune;
            this.input.OnPreferredControllerChanged += this.PreferredControllerChanged;
            this.modelRunner.OnPredictionReceived += this.PredictionReceived;

            // set preferred controller from save Data (default: right)
            SaveData saveData = SaveGameManager.Instance.GetSaveData();
            this.PreferredController = saveData.preferredHand;
            //this.availableSpells = saveData.availableSpells; 
            this.availableSpells = spellCasting.GetAvailableSpells(); // Taken out the save data
        }


        private void Update()
        {
            switch (this.currentSpell)
            {
                case SpellCasting.Spell.Earth:
                    RaycastHit hit = this.input.GetWandSelection();
                    GameObject selectedObject = hit.collider != null ? hit.collider.gameObject : null;

                    if (selectedObject != null && selectedObject.CompareTag("Floor"))
                    {
                        this.spellCasting.SetSpellCastingTarget(hit.point);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }
                    else if (selectedObject != null && selectedObject.CompareTag("StonePlatform"))
                    {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(selectedObject);
                        this.SetLastSelectedObject(selectedObject);
                    }
                    else
                    {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }

                    break;
                case SpellCasting.Spell.Time:
                    // Select object for Time Spell
                    hit = this.input.GetWandSelection();
                    selectedObject = hit.collider != null ? hit.collider.gameObject : null;

                    if (selectedObject != null && selectedObject.CompareTag("TimeTarget"))
                    {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(selectedObject);
                        this.SetLastSelectedObject(selectedObject);
                    }
                    else
                    {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }

                    break;
                case SpellCasting.Spell.Air:
                    hit = this.input.GetWandSelection();
                    selectedObject = hit.collider != null ? hit.collider.gameObject : null;

                    if (selectedObject != null && !DisallowAirCastOnGround && selectedObject.CompareTag("Floor"))
                    {
                        this.spellCasting.SetSpellCastingTarget(hit.point);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }
                    else
                    {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }

                    break;
                default:
                    this.ResetLastSelectedObject();
                    this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                    this.spellCasting.SetSpecialCasting(null);
                    break;
            }
        }

        /// <summary>
        /// Resets the last selected object when the Rayinteractor is no longer hovering over it.
        /// </summary>
        private void ResetLastSelectedObject()
        {
            if (this.lastSelectedObject != null)
            {
                Outline outline = this.lastSelectedObject.GetComponent<Outline>();
                this.lastSelectedObject = null;

                if (outline != null)
                {
                    outline.enabled = false;
                }
            }
        }

        /// <summary>
        /// Sets the last selected/hovered object and enables the outline if available.
        /// </summary>
        /// <param name="gameObject">GameObject that was selected.</param>
        private void SetLastSelectedObject(GameObject gameObject)
        {
            this.ResetLastSelectedObject();
            this.lastSelectedObject = gameObject;
            Outline outline = this.lastSelectedObject.GetComponent<Outline>();

            if (outline != null)
            {
                outline.enabled = true;
            }
        }

        /// <summary>
        /// Called when the AI Model has finished predicting a rune.
        /// </summary>
        /// <param name="runeClass">Class of the predicted rune.</param>
        private void PredictionReceived(int runeClass)
        {
            // Note: Current model as of 07-apr-2023 - 0: Time, 1: Air, 2: Other

            switch (runeClass)
            {
                case 0:
                    // Time Spell
                    this.currentSpell = SpellCasting.Spell.Time;
                    break;
                case 1:
                    // Air Spell
                    this.currentSpell = SpellCasting.Spell.Air;
                    break;
                case 2:
                    // Fire Spell
                    this.currentSpell = SpellCasting.Spell.Fire;
                    break;
                case 3:
                    // Earth Spell
                    this.currentSpell = SpellCasting.Spell.Earth;
                    break;
                case 4:
                    // Water Spell
                    this.currentSpell = SpellCasting.Spell.Water;
                    break;
                case 5:
                    // Lightning Spell
                    this.currentSpell = SpellCasting.Spell.Lightning;
                    break;
                default:
                    // Unknown Rune
                    this.currentSpell = SpellCasting.Spell.None;
                    break;
            }

            // You may cast the spell
            this.input.OnControllerTrigger -= this.DrawRune;
            this.input.OnControllerTrigger += this.CastSpell;

            if (!this.availableSpells.Contains(this.currentSpell)) 
            {
                this.currentSpell = SpellCasting.Spell.None;
                // Draw again.
                this.input.OnControllerTrigger -= this.CastSpell;
                this.input.OnControllerTrigger += this.DrawRune;

                return;
            }

            this.StartCoroutine(this.ShowRune());
            this.input.SetVisualGradientForActiveSpell(this.currentSpell, this.PreferredController);

            this.spellCasting.ChargeSpell(this.currentSpell, this.PreferredController);
        }

        /// <summary>
        /// Charges spell based on drawn rune.
        /// </summary>
        /// <param name="drawingPoints">List of points that were drawn.</param>
        /// <param name="controller">Controller that was used to draw.</param>
        private void ChargeSpell(Vector3[] drawingPoints, XRInputManager.Controller controller)
        {
            if (drawingPoints.Length != 20)
            {
                return;
            }

            this.modelRunner.IdentifyRune(drawingPoints);
        }

        /// <summary>
        /// IEnumerator to show the rune for a short time with fade out and scale up animation.
        /// </summary>
        private IEnumerator ShowRune()
        {
            switch (this.currentSpell)
            {
                case SpellCasting.Spell.Water:
                    this.runeSpriteRenderer.sprite = this.waterRune;
                    GameManager.Instance.PlaySound("WaterSplash");
                    break;
                case SpellCasting.Spell.Fire:
                    this.runeSpriteRenderer.sprite = this.fireRune;
                    GameManager.Instance.PlaySound("Matchbox");
                    break;
                case SpellCasting.Spell.Earth:
                    this.runeSpriteRenderer.sprite = this.earthRune;
                    GameManager.Instance.PlaySound("RockFall");
                    break;
                case SpellCasting.Spell.Air:
                    this.runeSpriteRenderer.sprite = this.airRune;
                    GameManager.Instance.PlaySound("Windblow");
                    break;
                case SpellCasting.Spell.Lightning:
                    this.runeSpriteRenderer.sprite = this.lightningRune;
                    GameManager.Instance.PlaySound("ElectricCharge");
                    break;
                case SpellCasting.Spell.Time:
                    this.runeSpriteRenderer.sprite = this.timeRune;
                    GameManager.Instance.PlaySound("RuneRecognized");
                    break;
                default:
                    this.runeSpriteRenderer.sprite = null;
                    GameManager.Instance.PlaySound("CrackingGlass");
                    break;
            }

            // fade out and scale down animation
            for (float i = 1, j = 1; i >= 0; i -= 0.03f, j -= 0.01f)
            {
                this.runeSpriteRenderer.color = new Color(1, 1, 1, i);
                this.runeSpriteRenderer.transform.localScale = new Vector3(j, j, j);
                yield return new WaitForSeconds(0.005f);
            }

            this.runeSpriteRenderer.sprite = null;
        }

        /// <summary>
        /// Casts the spell if the trigger is pressed or interrupts the casting if the trigger is released.
        /// </summary>
        /// <param name="triggerPressed">Whether the trigger is pressed (or released).</param>
        /// <param name="controller">Controller that was used to cast.</param>
        private void CastSpell(bool triggerPressed, XRInputManager.Controller controller)
        {
            if (controller != this.PreferredController)
            {
                return;
            }

            if (triggerPressed)
            {
                if (this.currentSpell != SpellCasting.Spell.None)
                {
                    // cast spell
                    this.spellCasting.CastSpell(this.currentSpell, controller);
                    this.currentSpell = SpellCasting.Spell.None;
                    this.input.SetVisualGradientForActiveSpell(this.currentSpell, this.PreferredController);
                    this.spellCasting.ChargeSpell(this.currentSpell, controller);
                }
            }
            else
            {
                this.spellCasting.InterruptCasting();
                this.input.OnControllerTrigger -= this.CastSpell;
                this.input.OnControllerTrigger += this.DrawRune;
            }
        }

        /// <summary>
        /// Initiates drawing of a rune. Calls StartDrawing/StopDrawing on the DrawScript.
        /// </summary>
        /// <param name="axis">Axis of the controller.</param>
        /// <param name="clicked">Whether the trigger is pressed (or released).</param>
        /// <param name="controller">Controller that was used to draw.</param>
        //private void DrawRune(Vector2 axis, bool clicked, XRInputManager.Controller controller)
        private void DrawRune(bool clicked, XRInputManager.Controller controller)
        {
            if (controller != this.PreferredController)
            {
                return;
            }

            if (clicked)
            {
                this.drawScript.StartDrawing(controller);
            }
            else
            {
                this.drawScript.StopDrawing(controller);
            }
        }

        /// <summary>
        /// Called when the preferred controller is changed. Updates the wand ray interactor.
        /// </summary>
        /// <param name="controller">The new preferred controller.</param>
        private void PreferredControllerChanged(XRInputManager.Controller controller)
        {
            this.PreferredController = controller;
        }

        /// <summary>
        /// Set what Spells are available to the player.
        /// </summary>
        /// <param name="spells">List of available spells.</param>
        public void SetAvailableSpells(List<SpellCasting.Spell> spells)
        {
            // ToDo: "Freischalten pro level" implementieren
            this.availableSpells = spells;
        }

        /// <summary>
        /// Learn a new spell. Adds Spell to availableSpells and saves it to the SaveData.
        /// </summary>
        /// <param name="spell">Spell to learn/add.</param>
        public void LearnNewSpell(SpellCasting.Spell spell)
        {
            // check if spell is already known
            if (this.availableSpells.Contains(spell))
            {
                return;
            }

            this.availableSpells.Add(spell);
            SaveData saveData = SaveGameManager.Instance.GetSaveData();
            saveData.availableSpells = this.availableSpells;
            SaveGameManager.Save(saveData);
        }

        /// <summary>
        /// Resets the player to the spawn position and rotation.
        /// </summary>
        public void ResetPlayerToSpawnPosition()
        {
            this.origin.position = this.spawnPositionOrigin;
            this.origin.rotation = this.spawnRotationOrigin;
            this.head.position = this.spawnPositionHead;
            this.head.rotation = this.spawnRotationHead;
        }
    }
}