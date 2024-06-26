namespace Spellslinger.Game.XR
{
    using System.Collections;
    using Spellslinger.Game.Control;
    using Spellslinger.Game.Manager;
    using UnityEngine;
    using UnityEngine.XR;
    using UnityEngine.XR.Interaction.Toolkit;

    public class XRInputManager : MonoBehaviour {
        // XR Nodes
        private XRNode xrNodeRight = XRNode.RightHand;
        private XRNode xrNodeLeft = XRNode.LeftHand;
        private XRNode xrNodeHead = XRNode.Head;

        // XR devices
        private InputDevice rightController;
        private InputDevice leftController;
        private InputDevice headset;

        // XR Components
        [Header("XR Components")]
        [SerializeField] private XRRayInteractor rightControllerRayInteractor;
        [SerializeField] private XRRayInteractor leftControllerRayInteractor;
        [SerializeField] private XRRayInteractor rightGrabRayInteractor;
        [SerializeField] private XRRayInteractor leftGrabRayInteractor;
        [SerializeField] private XRInteractorLineVisual leftControllerLineVisual;
        [SerializeField] private XRInteractorLineVisual rightControllerLineVisual;
        [SerializeField] private ActionBasedSnapTurnProvider snapTurnProvider;
        [SerializeField] private UnityEngine.InputSystem.InputActionProperty rightHandTurn;
        [SerializeField] private UnityEngine.InputSystem.InputActionProperty leftHandTurn;
        private UnityEngine.InputSystem.InputActionProperty emptyAction;

        // XR Device States
        private bool lastTriggerButtonStateRight = false;
        private bool lastTriggerButtonStateLeft = false;
        private bool lastGripButtonStateRight = false;
        private bool lastGripButtonStateLeft = false;
        private bool lastTouchPadClickStateRight = false;
        private bool lastTouchPadClickStateLeft = false;
        private bool lastMenuButtonStateRight = false;
        private bool lastMenuButtonStateLeft = false;
        private Gradient invisibleGradient;
        private Gradient redGradient;
        private Gradient spellActiveGradient;

        // Draw Point (LineCast Origin)
        [Header("Draw Points")]
        [SerializeField] private GameObject drawPointLeft;
        [SerializeField] private GameObject drawPointRight;

        // Wand Material
        [Header("Wands & Wand Material")]
        [SerializeField] private Material wandMaterial;
        [SerializeField] private Texture2D wandGlowWater;
        [SerializeField] private Texture2D wandGlowFire;
        [SerializeField] private Texture2D wandGlowEarth;
        [SerializeField] private Texture2D wandGlowAir;
        [SerializeField] private Texture2D wandGlowLightning;
        [SerializeField] private Texture2D wandGlowTime;

        public enum Controller {
            Left,
            Right,
        }

        // Events
        public System.Action<bool, Controller> OnControllerTrigger { get; internal set; }
        public System.Action<float, Controller> OnControllerGrip { get; internal set; }
        public System.Action<Vector2, bool, Controller> OnControllerTouchpad { get; internal set; }
        public System.Action OnControllerMenu { get; internal set; }
        public System.Action<Controller> OnPreferredControllerChanged { get; internal set; }

        private IEnumerator Start() {
            // Create Gradient Colors for Line Visuals
            this.invisibleGradient = new Gradient();
            this.invisibleGradient.SetKeys(new[] { new GradientColorKey(Color.clear, 0f) }, new[] { new GradientAlphaKey(0f, 0f) });
            this.redGradient = new Gradient();
            this.redGradient.SetKeys(new[] { new GradientColorKey(Color.red, 0f) }, new[] { new GradientAlphaKey(1f, 0f) });
            this.spellActiveGradient = new Gradient();
            this.spellActiveGradient.SetKeys(new[] { new GradientColorKey(Color.yellow, 0f) }, new[] { new GradientAlphaKey(1f, 0f) });

            // Create empty Action (no turn based movement)
            this.emptyAction = default(UnityEngine.InputSystem.InputActionProperty);

            // Wait for Controllers to be initialized
            while (!this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)")) {
                yield return new WaitForSeconds(0.025f);
            }

            // Initialize Preferred Controller
            SaveData saveData = SaveGameManager.Instance.GetSaveData();
            this.SetPreferredController(saveData.preferredHand);

            // remove emission map from wand material
            this.wandMaterial.SetTexture("_EmissionMap", null);
            this.wandMaterial.SetColor("_EmissionColor", Color.black);
        }

        // Called when the object is enabled and active
        private void OnEnable() {
            if (!this.rightController.isValid || !this.leftController.isValid) {
                this.GetDevices();
            }
        }

        /// <summary>
        /// Gets all relevant XR devices (Right Controller, Left Controller, Headset).
        /// </summary>
        private void GetDevices() {
            if (!this.rightController.isValid) {
                this.rightController = InputDevices.GetDeviceAtXRNode(this.xrNodeRight);
            }

            if (!this.leftController.isValid) {
                this.leftController = InputDevices.GetDeviceAtXRNode(this.xrNodeLeft);
            }

            if (!this.headset.isValid) {
                this.headset = InputDevices.GetDeviceAtXRNode(this.xrNodeHead);
            }
        }

        // Called every frame
        private void Update() {
            // Check Events of Right Controller
            if (!this.rightController.isValid) {
                this.GetDevices();
            } else {
                // Capture TriggerButton
                bool triggerButtonPressed = false;
                bool triggerButtonReleased = false;

                if (this.rightController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed)) {
                    triggerButtonReleased = !triggerButtonPressed && this.lastTriggerButtonStateRight;
                    this.lastTriggerButtonStateRight = triggerButtonPressed;

                    if (triggerButtonPressed) {
                        this.OnControllerTrigger?.Invoke(true, Controller.Right);
                    } else if (triggerButtonReleased) {
                        this.OnControllerTrigger?.Invoke(false, Controller.Right);
                    }
                }

                // Capture MenuButton
                bool menuButtonPressed = false;
                bool menuButtonReleased = false;

                if (this.rightController.TryGetFeatureValue(CommonUsages.menuButton, out menuButtonPressed)) {
                    menuButtonReleased = !menuButtonPressed && this.lastMenuButtonStateRight;
                    this.lastMenuButtonStateRight = menuButtonPressed;

                    if ((menuButtonPressed && !this.lastMenuButtonStateRight) || menuButtonReleased) {
                        this.OnControllerMenu?.Invoke();
                    }
                }

                // Capture GripButton
                float gripActionValue = 0;
                bool gripButtonReleased = false;

                if (this.rightController.TryGetFeatureValue(CommonUsages.grip, out gripActionValue)) {
                    gripButtonReleased = gripActionValue < 0.5f && this.lastGripButtonStateRight;
                    this.lastGripButtonStateRight = gripActionValue > 0.5f;

                    if (gripActionValue > 0.5f || gripButtonReleased) {
                        this.OnControllerGrip?.Invoke(gripActionValue, Controller.Right);
                    }
                }

                // Capture Touchpad Axis and Click
                Vector2 touchpadAxis = Vector2.zero;
                bool touchpadClickPressed = false;
                bool touchpadClickReleased = false;

                if (this.rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out touchpadAxis)) {
                    if (this.rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out touchpadClickPressed)) {
                        touchpadClickReleased = !touchpadClickPressed && this.lastTouchPadClickStateRight;
                        this.lastTouchPadClickStateRight = touchpadClickPressed;

                        if (touchpadClickPressed || touchpadClickReleased) {
                            this.OnControllerTouchpad?.Invoke(touchpadAxis, touchpadClickPressed, Controller.Right);
                        }
                    }
                }
            }

            // Check Events of Left Controller
            if (!this.leftController.isValid) {
                this.GetDevices();
            } else {
                // Capture TriggerButton
                bool triggerButtonPressed = false;
                bool triggerButtonReleased = false;

                if (this.leftController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed)) {
                    triggerButtonReleased = !triggerButtonPressed && this.lastTriggerButtonStateLeft;
                    this.lastTriggerButtonStateLeft = triggerButtonPressed;

                    if (triggerButtonPressed) {
                        this.OnControllerTrigger?.Invoke(true, Controller.Left);
                    } else if (triggerButtonReleased) {
                        this.OnControllerTrigger?.Invoke(false, Controller.Left);
                    }
                }

                // Capture MenuButton
                bool menuButtonPressed = false;
                bool menuButtonReleased = false;

                if (this.leftController.TryGetFeatureValue(CommonUsages.menuButton, out menuButtonPressed)) {
                    menuButtonReleased = !menuButtonPressed && this.lastMenuButtonStateLeft;
                    this.lastMenuButtonStateLeft = menuButtonPressed;

                    if ((menuButtonPressed && !this.lastMenuButtonStateLeft) || menuButtonReleased) {
                        this.OnControllerMenu?.Invoke();
                    }
                }

                // Capture GripButton
                float gripActionValue = 0;
                bool gripButtonReleased = false;

                if (this.leftController.TryGetFeatureValue(CommonUsages.grip, out gripActionValue)) {
                    gripButtonReleased = gripActionValue < 0.5f && this.lastGripButtonStateLeft;
                    this.lastGripButtonStateLeft = gripActionValue > 0.5f;

                    if (gripActionValue > 0.5f || gripButtonReleased) {
                        this.OnControllerGrip?.Invoke(gripActionValue, Controller.Left);
                    }
                }

                // Capture Touchpad Axis and Click
                Vector2 touchpadAxis = Vector2.zero;
                bool touchpadClickPressed = false;
                bool touchpadClickReleased = false;

                if (this.leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out touchpadAxis)) {
                    if (this.leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out touchpadClickPressed)) {
                        touchpadClickReleased = !touchpadClickPressed && this.lastTouchPadClickStateLeft;
                        this.lastTouchPadClickStateLeft = touchpadClickPressed;

                        if (touchpadClickPressed || touchpadClickReleased) {
                            this.OnControllerTouchpad?.Invoke(touchpadAxis, touchpadClickPressed, Controller.Left);
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Set the preferred controller for casting spells. The other controller will be used for movement/teleporting.
        /// </summary>
        /// <param name="controller">The preferred controller.</param>
        public void SetPreferredController(Controller controller) {
            if (controller == Controller.Left) {

                // Set Line Type for Controller Ray Interactor
                this.leftControllerRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
            
                // Enable Turning for Left Controller and Disable for right Controller
                this.snapTurnProvider.leftHandSnapTurnAction = this.leftHandTurn;
                this.snapTurnProvider.rightHandSnapTurnAction = this.emptyAction;

                // Switch Prefabs (Hand with Wand and Hand without Wand)
                if (this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)").gameObject.activeSelf) {
                    this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)").Find("HandWandPlaceholder").gameObject.SetActive(true);
                    this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)").Find("HandPlaceholder").gameObject.SetActive(false);

                    this.rightGrabRayInteractor.transform.Find("[RightGrabController] Model Parent").Find("XRControllerRight(Clone)").Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.rightGrabRayInteractor.transform.Find("[RightGrabController] Model Parent").Find("XRControllerRight(Clone)").Find("HandPlaceholder").gameObject.SetActive(true);

                    // Add Interaction Layer Mask 'Grabbable' for XR Ray Interactor of the Right Controller
                    this.rightGrabRayInteractor.interactionLayers = InteractionLayerMask.GetMask("Grabbable");

                    // Remove Interaction Layer Mask 'Grabbable' for XR Ray Interactor of the Left Controller
                    this.leftGrabRayInteractor.interactionLayers = InteractionLayerMask.GetMask("Nothing");
                }

                this.leftControllerRayInteractor.rayOriginTransform = this.drawPointLeft.transform;
                
                //disable rightcontrollerRayInteractor
                this.rightControllerRayInteractor.enabled = false;
            } else {
                
                // Set Line Type for Controller Ray Interactor
                this.rightControllerRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;

                // Enable Turning for Right Controller and Disable for left Controller
                this.snapTurnProvider.rightHandSnapTurnAction = this.rightHandTurn;
                this.snapTurnProvider.leftHandSnapTurnAction = this.emptyAction;

                // Switch Prefabs (Hand with Wand and Hand without Wand)
                if (this.rightGrabRayInteractor.transform.Find("[RightGrabController] Model Parent").Find("XRControllerRight(Clone)").gameObject.activeSelf) {
                    this.rightGrabRayInteractor.transform.Find("[RightGrabController] Model Parent").Find("XRControllerRight(Clone)").Find("HandWandPlaceholder").gameObject.SetActive(true);
                    this.rightGrabRayInteractor.transform.Find("[RightGrabController] Model Parent").Find("XRControllerRight(Clone)").Find("HandPlaceholder").gameObject.SetActive(false);

                    this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)").Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.leftGrabRayInteractor.transform.Find("[LeftGrabController] Model Parent").Find("XRControllerLeft(Clone)").Find("HandPlaceholder").gameObject.SetActive(true);

                    // Add Interaction Layer Mask 'Grabbable' for XR Ray Interactor of the Left Controller
                    this.leftGrabRayInteractor.interactionLayers = InteractionLayerMask.GetMask("Grabbable");

                    // Remove Interaction Layer Mask 'Grabbable' for XR Ray Interactor of the Right Controller
                    this.rightGrabRayInteractor.interactionLayers = InteractionLayerMask.GetMask("Nothing");
                }

                this.rightControllerRayInteractor.rayOriginTransform = this.drawPointRight.transform;
                //disable leftcontrollerRayInteractor
                this.leftControllerRayInteractor.enabled = false;
            }

            this.OnPreferredControllerChanged?.Invoke(controller);
        }

        /// <summary>
        /// Set the visual gradient for the active spell.
        /// </summary>
        /// <param name="spell">The active spell.</param>
        /// <param name="controller">The wand controller.</param>
        public void SetVisualGradientForActiveSpell(SpellCasting.Spell spell, Controller controller) {
            if (controller == Controller.Left) {
                this.leftControllerLineVisual.invalidColorGradient = spell != SpellCasting.Spell.None ? this.spellActiveGradient : this.invisibleGradient;
            } else {
                this.rightControllerLineVisual.invalidColorGradient = spell != SpellCasting.Spell.None ? this.spellActiveGradient : this.invisibleGradient;
            }

            switch (spell) {
                case SpellCasting.Spell.Fire:
                    // set emission map for fire spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowFire);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.Water:
                    // set emission map for water spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowWater);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.Earth:
                    // set emission map for earth spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowEarth);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.Air:
                    // set emission map for air spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowAir);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.Lightning:
                    // set emission map for lightning spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowLightning);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.Time:
                    // set emission map for time spell
                    this.wandMaterial.SetTexture("_EmissionMap", this.wandGlowTime);
                    this.wandMaterial.SetColor("_EmissionColor", Color.white);
                    break;
                case SpellCasting.Spell.None:
                    // set emission map for no spell
                    this.wandMaterial.SetTexture("_EmissionMap", null);
                    this.wandMaterial.SetColor("_EmissionColor", Color.black);
                    break;
            }
        }

        /// <summary>
        /// Returns RaycastHit of the wand ray interactor.
        /// </summary>
        /// <returns>RaycastHit of the wand ray interactor.</returns>
        public RaycastHit GetWandSelection() {
            RaycastHit hit;

            if (SaveGameManager.Instance.GetSaveData().preferredHand == Controller.Left) {
                this.leftControllerRayInteractor.TryGetCurrent3DRaycastHit(out hit);
            } else {
                this.rightControllerRayInteractor.TryGetCurrent3DRaycastHit(out hit);
            }

            return hit;
        }

        /// <summary>
        /// Set UI Mode (e.g. for Main Menu). Hides off hand and shows main hand with line visual.
        /// </summary>
        /// <param name="controller">Main hand controller.</param>
        public void SetUIMode(Controller controller) {
            if (controller == Controller.Left) {
                this.rightControllerRayInteractor.gameObject.SetActive(false);
                this.rightGrabRayInteractor.gameObject.SetActive(false);
                this.leftControllerRayInteractor.gameObject.SetActive(true);
                this.leftGrabRayInteractor.gameObject.SetActive(true);

                this.leftControllerLineVisual.invalidColorGradient = this.spellActiveGradient;
            } else {
                this.rightControllerRayInteractor.gameObject.SetActive(true);
                this.rightGrabRayInteractor.gameObject.SetActive(true);
                this.leftControllerRayInteractor.gameObject.SetActive(false);
                this.leftGrabRayInteractor.gameObject.SetActive(false);

                this.rightControllerLineVisual.invalidColorGradient = this.spellActiveGradient;
            }
        }
    }
}
