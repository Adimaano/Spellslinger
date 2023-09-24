using System.Collections;
using Spellslinger.AI;
using Spellslinger.Game.XR;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Spellslinger.Game.Control
{
    public class Player : MonoBehaviour {
        private XRInputManager input;
        private XRRayInteractor wandRayInteractor;
        private Draw drawScript;
        private ModelRunner modelRunner;
        private SpellCasting spellCasting;

        private SpriteRenderer runeSpriteRenderer;

        private SpellCasting.Spell currentSpell = SpellCasting.Spell.None;

        private GameObject lastSelectedObject;

        // Rune Sprites
        [Header("Rune Sprites")]
        [SerializeField] private Sprite waterRune;
        [SerializeField] private Sprite fireRune;
        [SerializeField] private Sprite earthRune;
        [SerializeField] private Sprite airRune;
        [SerializeField] private Sprite lightningRune;
        [SerializeField] private Sprite timeRune;

        public XRInputManager.Controller PreferredController { get; set; }

        // Start is called before the first frame update
        private void Start() {
            // find dependencies in scene
            this.input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();
            this.drawScript = GameObject.Find("-- XR --").GetComponent<Draw>();
            this.modelRunner = GameObject.Find("-- XR --").GetComponent<ModelRunner>();
            this.spellCasting = GameObject.Find("-- XR --").GetComponent<SpellCasting>();
            this.runeSpriteRenderer = GameObject.Find("HUD-Canvas").transform.Find("Rune").GetComponent<SpriteRenderer>();

            // initialize eventlisteners
            this.drawScript.OnDrawFinished += this.ChargeSpell;
            this.input.OnControllerTrigger += this.CastSpell;
            this.input.OnControllerTouchpad += this.DrawRune;
            this.input.OnPreferredControllerChanged += this.PreferredControllerChanged;
            this.modelRunner.OnPredictionReceived += this.PredictionReceived;

            // set preferred controller from player prefs (default: right)
            this.PreferredController = (XRInputManager.Controller)PlayerPrefs.GetInt("preferredController", 1);
            this.wandRayInteractor = this.input.GetWandRayInteractor();

            // this.wandRayInteractor.hoverEntered.AddListener(this.OnHoverEnter);
            // this.wandRayInteractor.hoverExited.AddListener(this.OnHoverExit);
        }

        private void Update() {
            if (this.currentSpell == SpellCasting.Spell.Earth) {
                RaycastHit hit;
                if (this.wandRayInteractor.TryGetCurrent3DRaycastHit(out hit)) {
                    if (hit.collider.gameObject.CompareTag("Floor")) {
                        this.spellCasting.SetSpellCastingTarget(hit.point);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    } else if (hit.collider.gameObject.CompareTag("StonePlatform")) {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(hit.collider.gameObject);
                        this.SetLastSelectedObject(hit.collider.gameObject);
                    } else {
                        this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                        this.spellCasting.SetSpecialCasting(null);
                        this.ResetLastSelectedObject();
                    }
                }
            } else {
                this.ResetLastSelectedObject();
                this.spellCasting.SetSpellCastingTarget(Vector3.zero);
                this.spellCasting.SetSpecialCasting(null);
            }
        }

        private void ResetLastSelectedObject() {
            if (this.lastSelectedObject != null) {
                Outline outline = this.lastSelectedObject.GetComponent<Outline>();
                this.lastSelectedObject = null;

                if (outline != null) {
                    outline.enabled = false;
                }
            }
        }

        private void SetLastSelectedObject(GameObject gameObject) {
            this.ResetLastSelectedObject();
            this.lastSelectedObject = gameObject;
            Outline outline = this.lastSelectedObject.GetComponent<Outline>();

            if (outline != null) {
                outline.enabled = true;
            }
        }

        private void PredictionReceived(int runeClass) {
            //Debug.Log(runeClass);
            
            // Note: Current model as of 07-apr-2023 - 0: Time, 1: Air, 2: Other
            switch (runeClass) {
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

            if (this.currentSpell != SpellCasting.Spell.None) {
                this.StartCoroutine(this.ShowRune());
                GameManager.Instance.PlaySound("RuneRecognized");
                this.input.SetVisualGradientForActiveSpell(this.currentSpell);
            }
            
            this.spellCasting.ChargeSpell(this.currentSpell, this.PreferredController);
        }

        /// <summary>
        /// Charges spell based on drawn rune.
        /// </summary>
        /// <param name="drawingPoints">List of points that were drawn.</param>
        /// <param name="controller">Controller that was used to draw.</param>
        private void ChargeSpell(Vector3[] drawingPoints, XRInputManager.Controller controller) {
            if (drawingPoints.Length != 20) {
                return;
            }

            this.modelRunner.IdentifyRune(drawingPoints);
        }

        // IEnumerator to show the rune for a short time with fade out and scale up animation
        private IEnumerator ShowRune() {
            switch (this.currentSpell) {
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
            for (float i = 1, j = 1; i >= 0; i -= 0.03f, j -= 0.01f) {
                this.runeSpriteRenderer.color = new Color(1, 1, 1, i);
                this.runeSpriteRenderer.transform.localScale = new Vector3(j, j, j);
                yield return new WaitForSeconds(0.005f);
            }

            this.runeSpriteRenderer.sprite = null;
        }

        private void CastSpell(bool triggerPressed, XRInputManager.Controller controller) {
            if (controller != this.PreferredController) {
                return;
            }

            if (triggerPressed) {
                if (this.currentSpell != SpellCasting.Spell.None) {
                    // cast spell
                    this.spellCasting.CastSpell(this.currentSpell, controller);
                    this.currentSpell = SpellCasting.Spell.None;
                    this.input.SetVisualGradientForActiveSpell(this.currentSpell);
                    this.spellCasting.ChargeSpell(this.currentSpell, controller);
                }
            } else {
                this.spellCasting.InterruptCasting();
            }
        }

        private void DrawRune(Vector2 axis, bool clicked, XRInputManager.Controller controller) {
            if (controller != this.PreferredController) {
                return;
            }

            if (clicked) {
                this.drawScript.StartDrawing(controller);
            } else {
                this.drawScript.StopDrawing(controller);
            }
        }

        private void PreferredControllerChanged(XRInputManager.Controller controller) {
            this.PreferredController = controller;

            // this.wandRayInteractor.hoverEntered.RemoveListener(this.OnHoverEnter);
            // this.wandRayInteractor.hoverExited.RemoveListener(this.OnHoverExit);

            this.wandRayInteractor = this.input.GetWandRayInteractor();

            // this.wandRayInteractor.hoverEntered.AddListener(this.OnHoverEnter);
            // this.wandRayInteractor.hoverExited.AddListener(this.OnHoverExit);
        }

        // private void OnHoverEnter(HoverEnterEventArgs args) {
        //     IXRHoverInteractable interactable = args.interactableObject;
        //     Outline outline = interactable.transform.gameObject.GetComponent<Outline>();
        //     Debug.Log("On Hover Enter");

        //     if (outline != null) {
        //         Debug.Log("outline found");
        //         outline.enabled = true;
        //     }
        // }

        // private void OnHoverExit(HoverExitEventArgs args) {
        //     IXRHoverInteractable interactable = args.interactableObject;
        //     Outline outline = interactable.transform.gameObject.GetComponent<Outline>();
        //     Debug.Log("On Hover Exit");

        //     if (outline != null) {
        //         Debug.Log("outline found");
        //         outline.enabled = false;
        //     }
        // }
    }
}