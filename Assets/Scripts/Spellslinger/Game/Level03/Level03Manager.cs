namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;
    using Spellslinger.Game.Control;

    public class Level03Manager : MonoBehaviour {
        void Start() {
            GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Time);
        }
    }
}
