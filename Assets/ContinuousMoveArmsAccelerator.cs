using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class ContinuousMoveArmsAccelerator : MonoBehaviour
{
    [SerializeField] private ActionBasedContinuousMoveProvider continuousMoveProvider;
     // Game Objects
    [SerializeField] private GameObject LeftHand;
    [SerializeField] private GameObject RightHand;

    //Vector3 Positions
    [SerializeField] private Vector3 PositionPreviousFrameLeftHand;
    [SerializeField] private Vector3 PositionPreviousFrameRightHand;
    [SerializeField] private Vector3 PositionCurrentFrameLeftHand;
    [SerializeField] private Vector3 PositionCurrentFrameRightHand;
    [SerializeField] private Vector3 PlayerPositionPreviousFrame;
    [SerializeField] private Vector3 PlayerPositionCurrentFrame;
    [SerializeField] private float HandSpeed;
    private float topSpeed = 7f;
    private float acceleration = 0.25f;
    private float deceleration = 0.04f;
    private float baseSpeed;
    private int deceleration_debouncecounter = 0;
    // Start is called before the first frame update
    void Start()
    {
        baseSpeed = continuousMoveProvider.moveSpeed;
        PositionPreviousFrameLeftHand = LeftHand.transform.position; //set previous positions
        PositionPreviousFrameRightHand = RightHand.transform.position;
    }

    // Update is called once per frame
    void Update()
    {
        // get positons of hands
        PositionCurrentFrameLeftHand = LeftHand.transform.position;
        PositionCurrentFrameRightHand = RightHand.transform.position;

        // position of player
        PlayerPositionCurrentFrame = transform.position;

        // get distance the hands and player has moved from last frame
        var playerDistanceMoved = Vector3.Distance(PlayerPositionCurrentFrame, PlayerPositionPreviousFrame);

        // get distance the hands and player has moved from last frame
        var leftHandDistanceMoved = Vector3.Distance(PositionPreviousFrameLeftHand, PositionCurrentFrameLeftHand);
        var rightHandDistanceMoved = Vector3.Distance(PositionPreviousFrameRightHand, PositionCurrentFrameRightHand);

        // aggregate to get hand speed
        HandSpeed = ((leftHandDistanceMoved - playerDistanceMoved) + (rightHandDistanceMoved - playerDistanceMoved));
        Debug.Log("handspeed:" + HandSpeed);
        deceleration_debouncecounter++;
        if( HandSpeed > 0.05 )
        {
            deceleration_debouncecounter = 0;
            if(continuousMoveProvider.moveSpeed < topSpeed)
            {
                continuousMoveProvider.moveSpeed += acceleration;
            }
            else
            {
                continuousMoveProvider.moveSpeed = topSpeed;
            }
        }
        else
        {
            if(deceleration_debouncecounter >= 90)
            {
                if(continuousMoveProvider.moveSpeed > baseSpeed)
                {
                    continuousMoveProvider.moveSpeed -= deceleration;
                }
                else
                {
                    continuousMoveProvider.moveSpeed = baseSpeed;
                }
                deceleration_debouncecounter=90;
            }
        }
        Debug.Log("movespeed:" + continuousMoveProvider.moveSpeed);

        // set previous position of hands for next frame
        PositionPreviousFrameLeftHand = PositionCurrentFrameLeftHand;
        PositionPreviousFrameRightHand = PositionCurrentFrameRightHand;
        // set player position previous frame
        PlayerPositionPreviousFrame = PlayerPositionCurrentFrame;
        
    }
}