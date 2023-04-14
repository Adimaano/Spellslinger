using UnityEngine;

public class SpellCasting : MonoBehaviour {
    // Spells / Particle Effects
    [SerializeField] private GameObject fireballPrefab;

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
        None,
    }

    private void Start() {
        this.spellCastingRight = GameObject.Find("DrawPointRight");
        this.spellCastingLeft = GameObject.Find("DrawPointLeft");
    }

    private void CastFireSpell1(GameObject spellOrigin) {
        GameObject fireball = Instantiate(this.fireballPrefab, spellOrigin.transform.position, Quaternion.identity);
        fireball.transform.LookAt(spellOrigin.transform.parent.transform.position);

        FireBallSpell spell = fireball.GetComponentInChildren<FireBallSpell>();
        spell.SpellDirection = spellOrigin.transform.forward;
    }

    public void CastSpell(Spell spell, XRInputManager.Controller controller) {
        GameObject spellOrigin = controller == XRInputManager.Controller.Right ? this.spellCastingRight : this.spellCastingLeft;

        switch (spell) {
            case Spell.Fire:
                this.CastFireSpell1(spellOrigin);
                break;
            default:
                this.CastFireSpell1(spellOrigin);
                break;
        }

        // TODO: until the model is fully trained, we will just cast a random spell and ignore the identified rune
    }
}
