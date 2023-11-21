using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClockSpint : MonoBehaviour
{
    [SerializeField] public GameObject arm;

    // Update is called once per frame
    void Update()
    {
        arm.transform.Rotate(0, 0, 5 * Time.deltaTime);
    }
}