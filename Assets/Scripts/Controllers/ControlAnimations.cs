using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Transform body;
    public Animator bodyAnim;

    public Animator weaponIKAnim;
    public Animator weaponAnim;

    public Transform weapon;
    public Transform leftHandTarget;
    public Transform rightHandTarget;

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
        weaponIKAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponAnim.SetBool("Aiming", inputState.playerIsAiming);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);

        bodyAnim.SetBool("Shooting", inputState.playerIsShooting);
        weaponAnim.SetBool("Shooting", inputState.playerIsShooting);

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
        weaponIKAnim.SetBool("WallRunningLeft", wrLeft);
        bodyAnim.SetBool("WallRunningRight", wrRight);
        weaponIKAnim.SetBool("WallRunningRight", wrRight);
        //Rotate our body to match the turned animation
        HandleBodyRotation(wrLeft, wrRight);
        //Fix issues with the wall-running animation weapon placement
        HandleWeaponPlacement(wrLeft, wrRight);
    }

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
            body.localRotation = currBodyRotation;
        }
    }

    void HandleWeaponPlacement(bool wrLeft, bool wrRight)
    {
        if (wrLeft)
        {
            weapon.SetParent(rightHandTarget);
            weapon.localPosition = rightHandWallRunWeaponPosition;
            weapon.localRotation = Quaternion.Euler(rightHandWallRunWeaponRotation);
        }
        else if (wrRight)
        {
            weapon.SetParent(leftHandTarget);
            weapon.localPosition = leftHandWallRunWeaponPosition;
            weapon.localRotation = Quaternion.Euler(leftHandWallRunWeaponRotation);
        }
        else
        {
            weapon.SetParent(rightHandTarget);
            weapon.localPosition = origWeaponPosition;
            weapon.localRotation = origWeaponRotation;
        }
    }
}