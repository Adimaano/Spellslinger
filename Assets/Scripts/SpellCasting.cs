using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpellCasting : MonoBehaviour {
    // Spells / Particle Effects
    [SerializeField] private GameObject fireballParticles;

    private GameObject spellCastingRight;
    private GameObject spellCastingLeft;

    // enum with all possible Spells
    public enum Spell { 
        Water,
        Fire,
        Earth,
        Air,
        Lightning,
        Time,
        None
    }

    private void Start() {
        spellCastingRight = GameObject.Find("DrawPointRight");
        spellCastingLeft = GameObject.Find("DrawPointLeft");
    }

    private void CastFireSpell1(GameObject spellOrigin) {
        GameObject fireball = Instantiate(fireballParticles, spellOrigin.transform.position, Quaternion.identity);
        fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);

        FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
        spell.SpellDirection = spellOrigin.transform.forward;
    }

    public void CastSpell(Spell spell, XRInputManager.Controller controller) {
        GameObject spellOrigin = controller == XRInputManager.Controller.Right ? spellCastingRight : spellCastingLeft;

        switch (spell) {
            case Spell.Fire:
                CastFireSpell1(spellOrigin);
                break;
            default:
                CastFireSpell1(spellOrigin);
                break;
        }

        // TODO: until the model is fully trained, we will just cast a random spell and ignore the identified rune
    }
}
