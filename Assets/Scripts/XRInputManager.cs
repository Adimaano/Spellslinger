using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR;

public class XRInputManager : MonoBehaviour {
    // XR Nodes
    private XRNode xrNodeRight = XRNode.RightHand;
    private XRNode xrNodeLeft = XRNode.LeftHand;
    private XRNode xrNodeHead = XRNode.Head;

    // XR devices
    private InputDevice rightController;
    private InputDevice leftController;
    private InputDevice headset;

    // XR Device States
    private bool lastTriggerButtonStateRight = false;
    private bool lastTriggerButtonStateLeft = false;
    private bool lastGripButtonStateRight = false;
    private bool lastGripButtonStateLeft = false;
    private bool lastTouchPadClickStateRight = false;
    private bool lastTouchPadClickStateLeft = false;

    public enum Controller {
        Left,
        Right,
    }

    // Events
    public System.Action<bool, Controller> OnControllerTrigger { get; internal set; }
    public System.Action<float, Controller> OnControllerGrip { get; internal set; }
    public System.Action<Vector2, bool, Controller> OnControllerTouchpad { get; internal set; }
    public System.Action<Vector2, bool> OnLeftControllerTouchpad { get; internal set; }

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

    // Called when the object is enabled and active
    private void OnEnable() {
        if (!this.rightController.isValid || !this.leftController.isValid) {
            this.GetDevices();
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
}
