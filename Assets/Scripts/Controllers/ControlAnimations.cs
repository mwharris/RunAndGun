using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public AnimationPosInfo crouchPosition;
    public AnimationPosInfo dhPosition;
    
    private Vector3 origLocalPos;

    private BodyController bodyControl;
    private PlayerBodyData playerBodyData;

    void Start()
    {
        //PlayerBodyData stores all info needed to control either our 1st or 3rd person body
        bodyControl = GetComponent<BodyController>();
        playerBodyData = bodyControl.PlayerBodyData;
        //Set original player body positions and rotations
        origLocalPos = playerBodyData.body.localPosition;
    }

    void Update()
    {
        playerBodyData = bodyControl.PlayerBodyData;
        WeaponData currWeaponData = playerBodyData.GetWeaponData();

        //Alias player body data information for later
        Animator bodyAnim = playerBodyData.GetBodyAnimator();
        //Animator weaponIKAnim = currWeaponData.WeaponIKAnimator;
        Animator weaponAnim = playerBodyData.weapon.GetComponent<Animator>();
        Animator thirdPersonAnim = bodyControl.ThirdPersonBody.GetBodyAnimator();

        //Set various Animator properties to control the animators properly
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);
        thirdPersonAnim.SetBool("Sprinting", inputState.playerIsSprinting);

        bodyAnim.SetBool("SingleHanded", inputState.playerWeaponStyle == WeaponStyles.SingleHanded);
        bodyAnim.SetBool("DoubleHanded", inputState.playerWeaponStyle == WeaponStyles.DoubleHanded);

        bodyAnim.SetBool("Aiming", inputState.playerIsAiming);
        thirdPersonAnim.SetBool("Aiming", inputState.playerIsAiming);
        /*if (weaponIKAnim.gameObject.activeSelf)
        {
            weaponIKAnim.SetBool("Aiming", inputState.playerIsAiming);
        }*/
        //weaponAnim.SetBool("Aiming", inputState.playerIsAiming);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);
        thirdPersonAnim.SetBool("Crouching", inputState.playerIsCrouching);

        if (inputState.playerIsShooting)
        {
            bodyAnim.SetTrigger("ShootTrig");
            thirdPersonAnim.SetTrigger("ShootTrig");
            weaponAnim.SetTrigger("ShootTrig");
        }

        if (inputState.playerIsReloading)
        {
            bodyAnim.SetTrigger("ReloadTrig");
            thirdPersonAnim.SetTrigger("ReloadTrig");
        }

        HandleWallRunningAnimations(bodyAnim, thirdPersonAnim, inputState.playerIsWallRunningLeft, inputState.playerIsWallRunningRight);

        bodyAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        bodyAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);
        bodyAnim.SetBool("JumpStart", inputState.playerIsJumping);
        thirdPersonAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        thirdPersonAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);
        thirdPersonAnim.SetBool("JumpStart", inputState.playerIsJumping);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        bodyAnim.SetFloat("ForwardSpeed", fwdSpeed);
        bodyAnim.SetFloat("SideSpeed", sideSpeed);
        thirdPersonAnim.SetFloat("ForwardSpeed", fwdSpeed);
        thirdPersonAnim.SetFloat("SideSpeed", sideSpeed);

        bodyAnim.SetFloat("LookAngle", inputState.playerLookAngle);
        thirdPersonAnim.SetFloat("LookAngle", inputState.playerLookAngle);
    }

    void HandleWallRunningAnimations(Animator bodyAnim, Animator thirdPersonAnim, bool wrLeft, bool wrRight)
    {
        //Tell both animators that we are wall-running
        bodyAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        bodyAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
        thirdPersonAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        thirdPersonAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
    }
}