﻿using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{    
    public Vector3 wallRunLeftTargetRot = Vector3.zero;
    public Vector3 wallRunRightTargetRot = Vector3.zero;
    private Quaternion origBodyRotation;
    private Quaternion currBodyRotation;

    public Vector3 leftHandWallRunWeaponPosition;
    public Vector3 leftHandWallRunWeaponRotation;
    public Vector3 rightHandWallRunWeaponPosition;
    public Vector3 rightHandWallRunWeaponRotation;
    private Vector3 origWeaponPosition;
    private Quaternion origWeaponRotation;

    private PlayerBodyData playerBodyData;

    void Start()
    {
        //PlayerBodyData stores all info needed to control either our 1st or 3rd person body
        playerBodyData = GetComponent<BodyController>().PlayerBodyData;
        //Store original body and weapon position / rotations for later calculations
        origBodyRotation = playerBodyData.body.localRotation;
        currBodyRotation = origBodyRotation;
        origWeaponPosition = playerBodyData.weapon.localPosition;
        origWeaponRotation = playerBodyData.weapon.localRotation;
    }

    void Update()
    {
        //Alias player body data information for later
        Animator bodyAnim = playerBodyData.bodyAnimator;
        Animator weaponIKAnim = playerBodyData.weaponIKAnim;
        Animator weaponAnim = playerBodyData.weaponAnim;

        //Set various Animator properties to control the animators properly
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);

        bodyAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponIKAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponAnim.SetBool("Aiming", inputState.playerIsAiming);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);

        bodyAnim.SetBool("Shooting", inputState.playerIsShooting);
        weaponAnim.SetBool("Shooting", inputState.playerIsShooting);

        bodyAnim.SetBool("Reloading", inputState.playerIsReloading);

        HandleWallRunningAnimations(bodyAnim, weaponIKAnim, inputState.playerIsWallRunningLeft, inputState.playerIsWallRunningRight);

        bodyAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        bodyAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        bodyAnim.SetFloat("ForwardSpeed", fwdSpeed);
        bodyAnim.SetFloat("SideSpeed", sideSpeed);

        bodyAnim.SetFloat("LookAngle", inputState.playerLookAngle);
    }

    void HandleWallRunningAnimations(Animator bodyAnim, Animator weaponIKAnim, bool wrLeft, bool wrRight)
    {
        //Tell the animator we are wall-running
        bodyAnim.SetBool("WallRunningLeft", wrLeft);
        weaponIKAnim.SetBool("WallRunningLeft", wrLeft);
        bodyAnim.SetBool("WallRunningRight", wrRight);
        weaponIKAnim.SetBool("WallRunningRight", wrRight);
        //Rotate our body to match the turned animation
        HandleBodyRotation(wrLeft, wrRight);
        //Fix issues with the wall-running animation weapon placement
        HandleWeaponPlacement(wrLeft, wrRight);
    }

    //Rotate the player's body 90-degrees depending on wall-run side.
    //This is to overcome an issue with the animations being rotated 90-degrees.
    //ANIMATIONS SHOULD BE FIXED AND THIS CODE SHOULD BE REMOVED
    void HandleBodyRotation(bool wrLeft, bool wrRight)
    {
        if (wrLeft || wrRight || origBodyRotation != currBodyRotation)
        {
            if (wrLeft)
            {
                Quaternion rot = Quaternion.Euler(wallRunLeftTargetRot.x, wallRunLeftTargetRot.y, wallRunLeftTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, Time.deltaTime * 5f);
            }
            else if (wrRight)
            {
                Quaternion rot = Quaternion.Euler(wallRunRightTargetRot.x, wallRunRightTargetRot.y, wallRunRightTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, Time.deltaTime * 5f);
            }
            else
            {
                currBodyRotation = Quaternion.Lerp(currBodyRotation, origBodyRotation, Time.deltaTime * 10f);
            }
            playerBodyData.body.localRotation = currBodyRotation;
        }
    }

    //When wall-running, one hand is on the wall and one hand aims/holds the weapon.
    //This will handle swapping the weapon to the opposite hand when wall-running right.
    //UPDATE THIS TO NOT HAPPEN IN FIRST-PERSON
    void HandleWeaponPlacement(bool wrLeft, bool wrRight)
    {
        if (wrLeft)
        {
            playerBodyData.weapon.SetParent(playerBodyData.rightHandTarget);
            playerBodyData.weapon.localPosition = rightHandWallRunWeaponPosition;
            playerBodyData.weapon.localRotation = Quaternion.Euler(rightHandWallRunWeaponRotation);
        }
        else if (wrRight)
        {
            playerBodyData.weapon.SetParent(playerBodyData.leftHandTarget);
            playerBodyData.weapon.localPosition = leftHandWallRunWeaponPosition;
            playerBodyData.weapon.localRotation = Quaternion.Euler(leftHandWallRunWeaponRotation);
        }
        else
        {
            //playerBodyData.weapon.SetParent(playerBodyData.rightHandTarget);
            //playerBodyData.weapon.localPosition = origWeaponPosition;
            //playerBodyData.weapon.localRotation = origWeaponRotation;
        }
    }
}