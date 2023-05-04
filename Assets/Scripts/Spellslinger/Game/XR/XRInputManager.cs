using Spellslinger.Game.Control;
using UnityEngine;
using UnityEngine.XR;
using UnityEngine.XR.Interaction.Toolkit;

namespace Spellslinger.Game.XR
{
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

        public enum Controller {
            Left,
            Right,
        }

        // Events
        public System.Action<bool, Controller> OnControllerTrigger { get; internal set; }
        public System.Action<float, Controller> OnControllerGrip { get; internal set; }
        public System.Action<Vector2, bool, Controller> OnControllerTouchpad { get; internal set; }
        public System.Action OnControllerMenu { get; internal set; }

        private void Start() {
            // Create Gradient Colors for Line Visuals
            this.invisibleGradient = new Gradient();
            this.invisibleGradient.SetKeys(new[] { new GradientColorKey(Color.clear, 0f) }, new[] { new GradientAlphaKey(0f, 0f) });
            this.redGradient = new Gradient();
            this.redGradient.SetKeys(new[] { new GradientColorKey(Color.red, 0f) }, new[] { new GradientAlphaKey(1f, 0f) });
            this.spellActiveGradient = new Gradient();
            this.spellActiveGradient.SetKeys(new[] { new GradientColorKey(Color.yellow, 0f) }, new[] { new GradientAlphaKey(1f, 0f) });

            // Create empty Action (no turn based movement)
            this.emptyAction = default(UnityEngine.InputSystem.InputActionProperty);

            // Initialize Preferred Controller
            this.SetPreferredController((Controller)PlayerPrefs.GetInt("preferredController", 1));
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
                // Add Interaction Layer Mask 'Teleport' for XR Ray Interactor of the Right Controller
                this.rightControllerRayInteractor.interactionLayers |= 1 << LayerMask.NameToLayer("Teleport");

                // Remove Interaction Layer Mask 'Teleport' for XR Ray Interactor of the Left Controller
                this.leftControllerRayInteractor.interactionLayers &= ~(1 << LayerMask.NameToLayer("Teleport"));

                // Set Line Type for controllers
                this.leftControllerRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
                this.rightControllerRayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
                this.rightControllerRayInteractor.velocity = 8.0f;

                // Set Invalid Line Visual Gradient for controllers
                this.leftControllerLineVisual.invalidColorGradient = this.invisibleGradient;
                this.rightControllerLineVisual.invalidColorGradient = this.redGradient;

                // Enable Turning for Left Controller and Disable for right Controller
                this.snapTurnProvider.leftHandSnapTurnAction = this.emptyAction;
                this.snapTurnProvider.rightHandSnapTurnAction = this.rightHandTurn;

                // Switch Prefabs (Hand with Wand and Hand without Wand)
            
                if (this.leftControllerRayInteractor.transform.Find("[LeftController] Model Parent").transform.Find("XRControllerLeft(Clone)")) {
                    this.leftControllerRayInteractor.transform.Find("[LeftController] Model Parent").transform.Find("XRControllerLeft(Clone)").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                    this.leftControllerRayInteractor.transform.Find("[LeftController] Model Parent").transform.Find("XRControllerLeft(Clone)").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                    this.rightControllerRayInteractor.transform.Find("[RightController] Model Parent").transform.Find("XRControllerRight(Clone)").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.rightControllerRayInteractor.transform.Find("[RightController] Model Parent").transform.Find("XRControllerRight(Clone)").transform.Find("HandPlaceholder").gameObject.SetActive(true);

                    this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandPlaceholder").gameObject.SetActive(false);
                    this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandPlaceholder").gameObject.SetActive(false);
                } else {
                    if (this.leftControllerRayInteractor.transform.Find("XRControllerLeft").gameObject.activeSelf) {
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                        this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                        this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandPlaceholder").gameObject.SetActive(true);
                    } else {
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeftOculusPackage").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeftOculusPackage").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                        this.rightControllerRayInteractor.transform.Find("XRControllerRightOculusPackage").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                        this.rightControllerRayInteractor.transform.Find("XRControllerRightOculusPackage").transform.Find("HandPlaceholder").gameObject.SetActive(true);
                    }
                }

                this.leftControllerRayInteractor.rayOriginTransform = this.drawPointLeft.transform;
                this.rightControllerRayInteractor.rayOriginTransform = this.rightControllerRayInteractor.gameObject.transform;
            } else {
                // Add Interaction Layer Mask 'Teleport' for XR Ray Interactor of the Left Controller
                this.leftControllerRayInteractor.interactionLayers |= 1 << LayerMask.NameToLayer("Teleport");

                // Remove Interaction Layer Mask 'Teleport' for XR Ray Interactor of the Right Controller
                this.rightControllerRayInteractor.interactionLayers &= ~(1 << LayerMask.NameToLayer("Teleport"));

                // Set Line Type for controllers
                this.rightControllerRayInteractor.lineType = XRRayInteractor.LineType.StraightLine;
                this.leftControllerRayInteractor.lineType = XRRayInteractor.LineType.ProjectileCurve;
                this.leftControllerRayInteractor.velocity = 8.0f;

                // Set Invalid Line Visual Gradient for controllers
                this.rightControllerLineVisual.invalidColorGradient = this.invisibleGradient;
                this.leftControllerLineVisual.invalidColorGradient = this.redGradient;

                // Enable Turning for Right Controller and Disable for left Controller
                this.snapTurnProvider.rightHandSnapTurnAction = this.emptyAction;
                this.snapTurnProvider.leftHandSnapTurnAction = this.leftHandTurn;

                // Switch Prefabs (Hand with Wand and Hand without Wand)
                if (this.rightControllerRayInteractor.transform.Find("[RightController] Model Parent").transform.Find("XRControllerRight(Clone)")) {
                    this.rightControllerRayInteractor.transform.Find("[RightController] Model Parent").transform.Find("XRControllerRight(Clone)").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                    this.rightControllerRayInteractor.transform.Find("[RightController] Model Parent").transform.Find("XRControllerRight(Clone)").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                    this.leftControllerRayInteractor.transform.Find("[LeftController] Model Parent").transform.Find("XRControllerLeft(Clone)").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.leftControllerRayInteractor.transform.Find("[LeftController] Model Parent").transform.Find("XRControllerLeft(Clone)").transform.Find("HandPlaceholder").gameObject.SetActive(true);

                    this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandPlaceholder").gameObject.SetActive(false);
                    this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                    this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandPlaceholder").gameObject.SetActive(false);
                } else {
                    if (this.rightControllerRayInteractor.transform.Find("XRControllerRight").gameObject.activeSelf) {
                        this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                        this.rightControllerRayInteractor.transform.Find("XRControllerRight").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                        this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeft").transform.Find("HandPlaceholder").gameObject.SetActive(true);
                    } else {
                        this.rightControllerRayInteractor.transform.Find("XRControllerRightOculusPackage").transform.Find("HandWandPlaceholder").gameObject.SetActive(true);
                        this.rightControllerRayInteractor.transform.Find("XRControllerRightOculusPackage").transform.Find("HandPlaceholder").gameObject.SetActive(false);

                        this.leftControllerRayInteractor.transform.Find("XRControllerLeftOculusPackage").transform.Find("HandWandPlaceholder").gameObject.SetActive(false);
                        this.leftControllerRayInteractor.transform.Find("XRControllerLeftOculusPackage").transform.Find("HandPlaceholder").gameObject.SetActive(true);
                    }
                }

                this.leftControllerRayInteractor.rayOriginTransform = this.leftControllerRayInteractor.gameObject.transform;
                this.rightControllerRayInteractor.rayOriginTransform = this.drawPointRight.transform;
            }
        }

        public void SetVisualGradientForActiveSpell(SpellCasting.Spell spell) {
            if ((Controller)PlayerPrefs.GetInt("preferredController", 1) == Controller.Left) {
                this.leftControllerLineVisual.invalidColorGradient = spell != SpellCasting.Spell.None ? this.spellActiveGradient : this.invisibleGradient;
            } else {
                this.rightControllerLineVisual.invalidColorGradient = spell != SpellCasting.Spell.None ? this.spellActiveGradient : this.invisibleGradient;
            }
        }
    }
}
