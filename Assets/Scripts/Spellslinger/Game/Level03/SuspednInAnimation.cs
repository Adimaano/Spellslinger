namespace Spellslinger.Game {
    using System.Collections;
    using System.Collections.Generic;
    using UnityEngine;

    public class SuspednInAnimation : MonoBehaviour
    {
        [SerializeField] public Animator anim;
        public void Suspend()
        {
            Debug.Log("Suspend");
            anim.StartPlayback();
            anim.Play("Default", -1, 0.98F);
        }
    }
}