using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Transform body;
    public Animator bodyAnim;

    public Animator weaponAnim;
    public Transform weapon;
    public Transform leftHandTarget;
    public Transform rightHandTarget;
    private Vector3 origWeaponPosition;
    private Quaternion origWeaponRotation;

    public Vector3 wallRunLeftTargetRot = Vector3.zero;
    public Vector3 wallRunRightTargetRot = Vector3.zero;
    private Quaternion origBodyRotation;
    private Quaternion currBodyRotation;

    void Start()
    {
        origBodyRotation = body.localRotation;
        currBodyRotation = origBodyRotation;

        origWeaponPosition = weapon.localPosition;
        origWeaponRotation = weapon.localRotation;
    }

    void Update()
    {        
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);
        
        bodyAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponAnim.SetBool("Aiming", inputState.playerIsAiming);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);

        bodyAnim.SetBool("Shooting", inputState.playerIsShooting);

        bodyAnim.SetBool("Reloading", inputState.playerIsReloading);

        HandleWallRunningAnimations(inputState.playerIsWallRunningLeft, inputState.playerIsWallRunningRight);

        bodyAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        bodyAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        bodyAnim.SetFloat("ForwardSpeed", fwdSpeed);
        bodyAnim.SetFloat("SideSpeed", sideSpeed);

        bodyAnim.SetFloat("LookAngle", inputState.playerLookAngle);
    }

    void HandleWallRunningAnimations(bool wrLeft, bool wrRight)
    {
        //Tell the animator we are wall-running
        bodyAnim.SetBool("WallRunningLeft", wrLeft);
        weaponAnim.SetBool("WallRunningLeft", wrLeft);
        bodyAnim.SetBool("WallRunningRight", wrRight);
        weaponAnim.SetBool("WallRunningRight", wrRight);
        if (wrLeft || wrRight || origBodyRotation != currBodyRotation)
        {
            //Rotate the body left/right to match the animation being played
            if (wrLeft)
            {
                Quaternion rot = Quaternion.Euler(wallRunLeftTargetRot.x, wallRunLeftTargetRot.y, wallRunLeftTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, Time.deltaTime * 5f); 
            }
            else if (wrRight)
            {
                Quaternion rot = Quaternion.Euler(wallRunRightTargetRot.x, wallRunRightTargetRot.y, wallRunRightTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, Time.deltaTime * 5f);
                HandleWeaponHandSwap(leftHandTarget, false);
            }
            else
            {
                currBodyRotation = Quaternion.Lerp(currBodyRotation, origBodyRotation, Time.deltaTime * 10f);
            }
            body.localRotation = currBodyRotation;
        }
        //Reset the weapon back to the right hand once we stop wall-running
        if (!wrLeft && !wrRight && weapon.parent == leftHandTarget)
        {
            HandleWeaponHandSwap(rightHandTarget, true);
        }
    }

    //Helper function to switch the weapon between hands when wall-running
    void HandleWeaponHandSwap(Transform newParent, bool fixPlacement)
    {
        weapon.SetParent(newParent);
        //When returning to the right hand we need to fix the weapon placement
        if (fixPlacement)
        {
            weapon.localPosition = origWeaponPosition;
            weapon.localRotation = origWeaponRotation;
        }
    }
}