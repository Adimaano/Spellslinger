using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class testContr : MonoBehaviour
{
    [SerializeField] public Animator anim;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetButton("Jump")) // equivalent to "trigger"
        {
            if(Input.GetButton("Fire3"))
            {
                anim.speed = -0.5F; // value shall changed depending on delta (geschwindigkeit) movement of the controller toward <-- of center(position of when trigger was pressed)
            }
            else if(Input.GetButton("Fire2"))
            {
                anim.speed = 0.5F; // value shall changed depending on delta (geschwindigkeit) movement of the controller toward --> of center(position of when trigger was pressed)
            }
            else
            {
                anim.speed = 0.0F; // value shall be 0 when trigger is held without moving the controller (0 geschwindigkeit)
            }
        }
    }
}
