using System.Collections;
using UnityEngine;

public class Player : MonoBehaviour {
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
    
    // Start is called before the first frame update
    private void Start() {
        // find dependencies in scene
        input = GameObject.Find("-- XR --").GetComponent<XRInputManager>();
        drawScript = GameObject.Find("-- XR --").GetComponent<Draw>();
        modelRunner = GameObject.Find("-- XR --").GetComponent<ONNXModelRunner>();
        spellCasting = GameObject.Find("-- XR --").GetComponent<SpellCasting>();
        runeSpriteRenderer = GameObject.Find("HUD-Canvas").transform.Find("Rune").GetComponent<SpriteRenderer>();

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
        if (drawingPoints.Length != 20) { return; }
        int runeClass = modelRunner.IdentifyRune(drawingPoints);
        
        // Note: Current model as of 07-apr-2023 - 0: Time, 1: Air, 2: Other
        // Debug.Log("Identified Rune: " + runeClass);

        switch (runeClass) {
            case 0:
                // Time Spell
                currentSpell = SpellCasting.Spell.Time;
                break;
            case 1:
                // Air Spell 
                currentSpell = SpellCasting.Spell.Air;
                break;
            case 2:
                // Earth Spell
                currentSpell = SpellCasting.Spell.Earth;
                break;
            case 3:
                // Fire Spell
                currentSpell = SpellCasting.Spell.Fire;
                break;
            case 4:
                // Lightning Spell
                currentSpell = SpellCasting.Spell.Lightning;
                break;
            case 5:
                // Water Spell
                currentSpell = SpellCasting.Spell.Water;
                break;
            default:
                // Unknown Rune
                currentSpell = SpellCasting.Spell.None;
                break;
        }

        if (currentSpell != SpellCasting.Spell.None) {
            StartCoroutine(ShowRune());
            GameManager.Instance.PlaySound("RuneRecognized");
        }
    }

    // IEnumerator to show the rune for a short time with fade out and scale up animation
    private IEnumerator ShowRune() {
        switch (currentSpell) {
            case SpellCasting.Spell.Water:
                runeSpriteRenderer.sprite = waterRune;
                break;
            case SpellCasting.Spell.Fire:
                runeSpriteRenderer.sprite = fireRune;
                break;
            case SpellCasting.Spell.Earth:
                runeSpriteRenderer.sprite = earthRune;
                break;
            case SpellCasting.Spell.Air:
                runeSpriteRenderer.sprite = airRune;
                break;
            case SpellCasting.Spell.Lightning:
                runeSpriteRenderer.sprite = lightningRune;
                break;
            case SpellCasting.Spell.Time:
                runeSpriteRenderer.sprite = timeRune;
                break;
            default:
                runeSpriteRenderer.sprite = null;
                break;
        }

        // fade out and scale down animation
        for (float i=1, j=1; i >= 0; i -= 0.03f, j -= 0.01f) {
            runeSpriteRenderer.color = new Color(1, 1, 1, i);
            runeSpriteRenderer.transform.localScale = new Vector3(j, j, j);
            yield return new WaitForSeconds(0.005f);
        }

        runeSpriteRenderer.sprite = null;
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
