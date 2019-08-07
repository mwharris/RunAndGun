using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothReturn : AbstractBehavior
{
    public AnimationPosInfo crouchPosition;
    public AnimationPosInfo dhPosition;

    private Vector3 origLocalPos;

    [SerializeField] private float dhLerpReturnSpeed = 0f;
    [SerializeField] private float dhAimLerpMultiplier = 2f;

    void Start ()
    {
        //Set original player body positions and rotations
        origLocalPos = transform.localPosition;
    }
	
	void Update () {
        HandleBodyPlacement(inputState.playerIsAiming);
    }

    void HandleBodyPlacement(bool isAiming)
    {
        //Used for fast firing weapons.
        bool isShootDown = inputState.GetButtonPressed(inputs[0]);
        //Lerp variables
        float lerpSpeed = Time.deltaTime * 20f;
        Vector3 currPos = transform.localPosition;
        Quaternion currRot = transform.localRotation;

        if (inputState.playerIsCrouching)
        {
            //If both Aiming and Crouching, return to normal position.
            //BobController will handle the aiming.
            if (isAiming)
            {
                currPos = Vector3.Lerp(currPos, origLocalPos, lerpSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(new Vector3(0, 0, 0)), lerpSpeed);
            }
            else
            {
                currPos = Vector3.Lerp(currPos, crouchPosition.localPos, Time.deltaTime * 10f);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(crouchPosition.localRot), Time.deltaTime * 10f);
            }
        }
        else if (inputState.playerWeaponStyle == WeaponStyles.DoubleHanded)
        {
            if (isAiming)
            {
                currPos = Vector3.Lerp(currPos, dhPosition.localPos, dhLerpReturnSpeed * dhAimLerpMultiplier);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(dhPosition.localRot), dhLerpReturnSpeed * dhAimLerpMultiplier);
            }
            else
            {
                currPos = Vector3.Lerp(currPos, dhPosition.localPos, dhLerpReturnSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(dhPosition.localRot), dhLerpReturnSpeed);
            }
        }
        else
        {
            currPos = Vector3.Lerp(currPos, origLocalPos, lerpSpeed);
            currRot = Quaternion.Lerp(currRot, Quaternion.Euler(new Vector3(0, 0, 0)), lerpSpeed);
        }

        transform.localPosition = currPos;
        transform.localRotation = currRot;
    }
}
