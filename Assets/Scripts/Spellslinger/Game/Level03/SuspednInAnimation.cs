namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SuspednInAnimation : MonoBehaviour
    {
        [SerializeField] public Animator anim;

        public void Rewind()
        {
            Debug.Log("Rewind");
            anim.PlayInFixedTime("Default", -1, 2.0F);
        }
    }
}