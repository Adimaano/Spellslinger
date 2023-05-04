using System.Collections;
using Spellslinger.AI;
using Spellslinger.Game.XR;
using UnityEngine;

namespace Spellslinger.Game.Control
{
    public class Player : MonoBehaviour
    {
        private XRInputManager input;
        private Draw drawScript;
        private ONNXModelRunner modelRunner;
        private SpellCasting spellCasting;

        private SpriteRenderer runeSpriteRenderer;

        private SpellCasting.Spell currentSpell = SpellCasting.Spell.None;

        // Rune Sprites
        [SerializeField] private Sprite waterRune;
        [SerializeField] private Sprite fireRune;
        [SerializeField] private Sprite earthRune;
        [SerializeField] private Sprite airRune;
        [SerializeField] private Sprite lightningRune;
        [SerializeField] private Sprite timeRune;

        public XRInputManager.Controller PreferredController { get; set; }

        // Start is called before the first frame update
        private void Start()
        {
            // find dependencies in scene
            this.input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();
            this.drawScript = GameObject.Find("-- XR --").GetComponent<Draw>();
            this.modelRunner = GameObject.Find("-- XR --").GetComponent<ONNXModelRunner>();
            this.spellCasting = GameObject.Find("-- XR --").GetComponent<SpellCasting>();
            this.runeSpriteRenderer =
                GameObject.Find("HUD-Canvas").transform.Find("Rune").GetComponent<SpriteRenderer>();

            // initialize eventlisteners
            this.drawScript.OnDrawFinished += this.ChargeSpell;
            this.input.OnControllerTrigger += this.CastSpell;
            this.input.OnControllerTouchpad += this.DrawRune;

            // set preferred controller from player prefs (default: right)
            this.PreferredController = (XRInputManager.Controller)PlayerPrefs.GetInt("preferredController", 1);
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

            int runeClass = this.modelRunner.IdentifyRune(drawingPoints);

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
                    // Earth Spell
                    this.currentSpell = SpellCasting.Spell.Earth;
                    break;
                case 3:
                    // Fire Spell
                    this.currentSpell = SpellCasting.Spell.Fire;
                    break;
                case 4:
                    // Lightning Spell
                    this.currentSpell = SpellCasting.Spell.Lightning;
                    break;
                case 5:
                    // Water Spell
                    this.currentSpell = SpellCasting.Spell.Water;
                    break;
                default:
                    // Unknown Rune
                    this.currentSpell = SpellCasting.Spell.None;
                    break;
            }

            if (this.currentSpell != SpellCasting.Spell.None)
            {
                this.StartCoroutine(this.ShowRune());
                GameManager.Instance.PlaySound("RuneRecognized");
                this.input.SetVisualGradientForActiveSpell(this.currentSpell);
            }
        }

        // IEnumerator to show the rune for a short time with fade out and scale up animation
        private IEnumerator ShowRune()
        {
            switch (this.currentSpell)
            {
                case SpellCasting.Spell.Water:
                    this.runeSpriteRenderer.sprite = this.waterRune;
                    break;
                case SpellCasting.Spell.Fire:
                    this.runeSpriteRenderer.sprite = this.fireRune;
                    break;
                case SpellCasting.Spell.Earth:
                    this.runeSpriteRenderer.sprite = this.earthRune;
                    break;
                case SpellCasting.Spell.Air:
                    this.runeSpriteRenderer.sprite = this.airRune;
                    break;
                case SpellCasting.Spell.Lightning:
                    this.runeSpriteRenderer.sprite = this.lightningRune;
                    break;
                case SpellCasting.Spell.Time:
                    this.runeSpriteRenderer.sprite = this.timeRune;
                    break;
                default:
                    this.runeSpriteRenderer.sprite = null;
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

        private void CastSpell(bool triggerPressed, XRInputManager.Controller controller)
        {
            if (triggerPressed)
            {
                if (this.currentSpell != SpellCasting.Spell.None)
                {
                    // cast spell
                    this.spellCasting.CastSpell(this.currentSpell, controller);
                    this.currentSpell = SpellCasting.Spell.None;
                    this.input.SetVisualGradientForActiveSpell(this.currentSpell);
                }
            }
        }

        private void DrawRune(Vector2 axis, bool clicked, XRInputManager.Controller controller)
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
    }
}