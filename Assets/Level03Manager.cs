using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Level03Manager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        GameObject.Find("-- XR --").GetComponent<Player>().LearnNewSpell(SpellCasting.Spell.Time);
    }
}
