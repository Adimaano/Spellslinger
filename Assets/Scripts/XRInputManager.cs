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

    // Events
    public System.Action<bool, Controller> OnControllerTrigger { get; internal set; }
    public System.Action<float, Controller> OnControllerGrip { get; internal set; }
    public System.Action<Vector2, bool, Controller> OnControllerTouchpad { get; internal set; }
    public System.Action<Vector2, bool> OnLeftControllerTouchpad { get; internal set; }

    public enum Controller { Left, Right }

    /// <summary>
    /// Gets all relevant XR devices (Right Controller, Left Controller, Headset)
    /// </summary>
    /// <returns></returns>
    private void GetDevices() {
        if (!rightController.isValid) {
            rightController = InputDevices.GetDeviceAtXRNode(xrNodeRight);
        }

        if (!leftController.isValid) {
            leftController = InputDevices.GetDeviceAtXRNode(xrNodeLeft);
        }

        if (!headset.isValid) {
            headset = InputDevices.GetDeviceAtXRNode(xrNodeHead);
        }
    }

    // Called when the object is enabled and active
    private void OnEnable() {
        if (!rightController.isValid || !leftController.isValid) {
            GetDevices();
        }
    }

    // Called every frame
    private void Update() {
        // Check Events of Right Controller
        if (!rightController.isValid) {
            GetDevices();
        } else {
            // Capture TriggerButton
            bool triggerButtonPressed = false;
            bool triggerButtonReleased = false;

            if (rightController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed)) {
                triggerButtonReleased = !triggerButtonPressed && lastTriggerButtonStateRight;
                lastTriggerButtonStateRight = triggerButtonPressed;
                
                if (triggerButtonPressed) {
                    OnControllerTrigger?.Invoke(true, Controller.Right);
                } else if (triggerButtonReleased) {
                    OnControllerTrigger?.Invoke(false, Controller.Right);
                }
            }

            // Capture GripButton
            float gripActionValue = 0;
            bool gripButtonReleased = false;
            
            if (rightController.TryGetFeatureValue(CommonUsages.grip, out gripActionValue)) {
                gripButtonReleased = gripActionValue < 0.5f && lastGripButtonStateRight;
                lastGripButtonStateRight = gripActionValue > 0.5f;

                if (gripActionValue > 0.5f || gripButtonReleased) {
                    OnControllerGrip?.Invoke(gripActionValue, Controller.Right);
                }
            }

            // Capture Touchpad Axis and Click
            Vector2 touchpadAxis = Vector2.zero;
            bool touchpadClickPressed = false;
            bool touchpadClickReleased = false;
            
            if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxis, out touchpadAxis)) {
                if (rightController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out touchpadClickPressed)) {
                    touchpadClickReleased = !touchpadClickPressed && lastTouchPadClickStateRight;
                    lastTouchPadClickStateRight = touchpadClickPressed;

                    if (touchpadClickPressed || touchpadClickReleased) {
                        OnControllerTouchpad?.Invoke(touchpadAxis, touchpadClickPressed, Controller.Right);
                    }
                }
            }
        }

        // Check Events of Left Controller
        if (!leftController.isValid) {
            GetDevices();
        } else {
            // Capture TriggerButton
            bool triggerButtonPressed = false;
            bool triggerButtonReleased = false;

            if (leftController.TryGetFeatureValue(CommonUsages.triggerButton, out triggerButtonPressed)) {
                triggerButtonReleased = !triggerButtonPressed && lastTriggerButtonStateLeft;
                lastTriggerButtonStateLeft = triggerButtonPressed;

                if (triggerButtonPressed) {
                    OnControllerTrigger?.Invoke(true, Controller.Left);
                } else if (triggerButtonReleased) {
                    OnControllerTrigger?.Invoke(false, Controller.Left);
                }
            }

            // Capture GripButton
            float gripActionValue = 0;
            bool gripButtonReleased = false;

            if (leftController.TryGetFeatureValue(CommonUsages.grip, out gripActionValue)) {
                gripButtonReleased = gripActionValue < 0.5f && lastGripButtonStateLeft;
                lastGripButtonStateLeft = gripActionValue > 0.5f;

                if (gripActionValue > 0.5f || gripButtonReleased) {
                    OnControllerGrip?.Invoke(gripActionValue, Controller.Left);
                }
            }

            // Capture Touchpad Axis and Click
            Vector2 touchpadAxis = Vector2.zero;
            bool touchpadClickPressed = false;
            bool touchpadClickReleased = false;

            if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxis, out touchpadAxis)) {
                if (leftController.TryGetFeatureValue(CommonUsages.primary2DAxisClick, out touchpadClickPressed)) {
                    touchpadClickReleased = !touchpadClickPressed && lastTouchPadClickStateLeft;
                    lastTouchPadClickStateLeft = touchpadClickPressed;

                    if (touchpadClickPressed || touchpadClickReleased) {
                        OnControllerTouchpad?.Invoke(touchpadAxis, touchpadClickPressed, Controller.Left);
                    }
                }
            }
        }
    }
}
