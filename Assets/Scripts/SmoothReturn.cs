using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SmoothReturn : AbstractBehavior
{
    [SerializeField] private BodyController bodyController;
    [SerializeField] private float dhLerpReturnSpeed = 0f;
    [SerializeField] private float dhAimLerpMultiplier = 2f;

    private Vector3 currWeaponDefaultPos;
    private Vector3 currWeaponDefaultRot;
    private Vector3 currWeaponCrouchPos;
    private Vector3 currWeaponCrouchRot;
    private float currWeaponLerpSpeed = 20f;
    private float currWeaponLerpMultiplier = 0f;

    private float defaultLerpSpeed;
    private float defaultCrouchLerpSpeed;

    void Update ()
    {
        SetBodyVars(inputState.playerIsAiming);
        HandleBodyPlacement(inputState.playerIsAiming);
    }

    //Set our various position and rotation vectors based on the current weapon
    private void SetBodyVars(bool isAiming)
    {
        defaultLerpSpeed = Time.deltaTime * 20f;
        defaultCrouchLerpSpeed = Time.deltaTime * 10f;
        if (bodyController != null)
        {
            WeaponData currWeaponData = bodyController.PlayerBodyData.GetWeaponData();
            currWeaponDefaultPos = currWeaponData.DefaultArmsPosition.localPos;
            currWeaponDefaultRot = currWeaponData.DefaultArmsPosition.localRot;
            currWeaponCrouchPos = currWeaponData.CrouchArmsPosition.localPos;
            currWeaponCrouchRot = currWeaponData.CrouchArmsPosition.localRot;
            currWeaponLerpMultiplier = currWeaponData.KickReturnAimMultiplier;
            if (currWeaponData.KickReturnSpeed > 0)
            {
                currWeaponLerpSpeed = currWeaponData.KickReturnSpeed;
            }
            else if (inputState.playerIsCrouching && !isAiming)
            {
                currWeaponLerpSpeed = defaultCrouchLerpSpeed;
            }
            else
            {
                currWeaponLerpSpeed = defaultLerpSpeed;
            }
        }
    }

    void HandleBodyPlacement(bool isAiming)
    {
        //Used for fast firing weapons.
        bool isShootDown = inputState.GetButtonPressed(inputs[0]);
        //Lerp variables
        Vector3 currPos = transform.localPosition;
        Quaternion currRot = transform.localRotation;

        if (inputState.playerIsCrouching)
        {
            //If both Aiming and Crouching, return to normal position.
            //BobController will handle the aiming.
            if (isAiming)
            {
                if (currWeaponLerpMultiplier > 0)
                {
                    currWeaponLerpSpeed *= currWeaponLerpMultiplier;
                }
                currPos = Vector3.Lerp(currPos, currWeaponDefaultPos, currWeaponLerpSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(currWeaponDefaultRot), currWeaponLerpSpeed);
            }
            else
            {
                currPos = Vector3.Lerp(currPos, currWeaponCrouchPos, currWeaponLerpSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(currWeaponCrouchRot), currWeaponLerpSpeed);
            }
        }
        else
        {
            currPos = Vector3.Lerp(currPos, currWeaponDefaultPos, currWeaponLerpSpeed);
            currRot = Quaternion.Lerp(currRot, Quaternion.Euler(currWeaponDefaultRot), currWeaponLerpSpeed);
        }

        transform.localPosition = currPos;
        transform.localRotation = currRot;
    }
}
