using System.Collections.Generic;
using Spellslinger.Game.Control;
using Spellslinger.Game.XR;

[System.Serializable]
public class SaveData {
    // The difficulty level of the game
    public int currentLevel;

    // The learned/available spells
    public List<SpellCasting.Spell> availableSpells;

    // Preferred spellcasting hand
    public XRInputManager.Controller preferredHand;

    // A constructor for the save data
    public SaveData() {
        // Set the default values for the save data
        this.currentLevel = 1;
        this.availableSpells = new List<SpellCasting.Spell>();
        this.preferredHand = XRInputManager.Controller.Right;
    }
}
