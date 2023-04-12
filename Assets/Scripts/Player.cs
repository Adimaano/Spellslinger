using UnityEngine;

public class Player : MonoBehaviour {
    private XRInputManager input;
    private Draw drawScript;
    private ONNXModelRunner modelRunner;
    private SpellCasting spellCasting;

    private SpellCasting.Spell currentSpell = SpellCasting.Spell.None;
    
    // Start is called before the first frame update
    private void Start() {
        // find dependencies in scene
        input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();
        drawScript = GameObject.Find("-- XR --").GetComponent<Draw>();
        modelRunner = GameObject.Find("-- XR --").GetComponent<ONNXModelRunner>();
        spellCasting = GameObject.Find("-- XR --").GetComponent<SpellCasting>();

        // initialize eventlisteners
        drawScript.OnDrawFinished += ChargeSpell;
        input.OnControllerTrigger += CastSpell;
        input.OnControllerTouchpad += DrawRune;
    }

    /// <summary>
    /// Charges spell based on drawn rune
    /// </summary>
    /// <param name="drawingPoints">List of points that were drawn</param>
    /// <param name="controller">Controller that was used to draw</param>
    private void ChargeSpell(Vector3[] drawingPoints, XRInputManager.Controller controller) {
        int runeClass = modelRunner.IdentifyRune(drawingPoints);
        
        // Note: Current model as of 07-apr-2023 - 0: Time, 1: Air, 2: Other
        Debug.Log("Identified Rune: " + runeClass);

        switch (runeClass) {
            case 0:
                // Water Spell
                currentSpell = SpellCasting.Spell.Water;
                break;
            case 1:
                // Fire Spell
                currentSpell = SpellCasting.Spell.Fire;
                break;
            case 2:
                // Earth Spell
                currentSpell = SpellCasting.Spell.Earth;
                break;
            case 3:
                // Air Spell 
                currentSpell = SpellCasting.Spell.Air;
                break;
            case 4:
                // Lightning Spell
                currentSpell = SpellCasting.Spell.Lightning;
                break;
            case 5:
                // Time Spell
                currentSpell = SpellCasting.Spell.Time;
                break;
            default:
                // Unknown Rune
                currentSpell = SpellCasting.Spell.None;
                break;
        }
    }

    private void CastSpell(bool triggerPressed, XRInputManager.Controller controller) {
        if (triggerPressed) {
            if (currentSpell != SpellCasting.Spell.None) {
                // cast spell
                spellCasting.CastSpell(currentSpell, controller);
                currentSpell = SpellCasting.Spell.None;
            }
        }
    }

    private void DrawRune(Vector2 axis, bool clicked, XRInputManager.Controller controller) {
        if (clicked) {
            drawScript.StartDrawing(controller);
        } else {
            drawScript.StopDrawing(controller);
        }
    }
}
