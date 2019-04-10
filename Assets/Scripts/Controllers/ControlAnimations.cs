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

        //Alias player body data information for later
        Animator bodyAnim = playerBodyData.bodyAnimator;
        Animator weaponIKAnim = playerBodyData.weaponIKAnim;
        Animator weaponAnim = playerBodyData.weaponAnim;
        Animator thirdPersonAnim = bodyControl.ThirdPersonBody.bodyAnimator;

        //Set various Animator properties to control the animators properly
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);
        thirdPersonAnim.SetBool("Sprinting", inputState.playerIsSprinting);

        HandleBodyPlacement(inputState.playerIsAiming);

        bodyAnim.SetBool("SingleHanded", inputState.playerWeaponStyle == WeaponStyles.SingleHanded);
        bodyAnim.SetBool("DoubleHanded", inputState.playerWeaponStyle == WeaponStyles.DoubleHanded);

        bodyAnim.SetBool("Aiming", inputState.playerIsAiming);
        thirdPersonAnim.SetBool("Aiming", inputState.playerIsAiming);
        if (weaponIKAnim.gameObject.activeSelf)
        {
            weaponIKAnim.SetBool("Aiming", inputState.playerIsAiming);
        }
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

        HandleWallRunningAnimations(bodyAnim, thirdPersonAnim, weaponIKAnim, inputState.playerIsWallRunningLeft, inputState.playerIsWallRunningRight);

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

    void HandleBodyPlacement(bool isAiming)
    {
        float lerpSpeed = Time.deltaTime * 20f;
        Vector3 targetPos = Vector3.zero;
        Quaternion targetRot = Quaternion.identity;
        if (inputState.playerIsCrouching)
        {
            //If both Aiming and Crouching, return to normal position.
            //BobController will handle the aiming.
            if (isAiming)
            {
                targetPos = origLocalPos;
                targetRot = Quaternion.Euler(new Vector3(0, 0, 0));
            }
            else
            {
                targetPos = crouchPosition.localPos;
                targetRot = Quaternion.Euler(crouchPosition.localRot);
                lerpSpeed = Time.deltaTime * 10f;
            }
        }
        else if (inputState.playerWeaponStyle == WeaponStyles.DoubleHanded)
        {
            targetPos = dhPosition.localPos;
            targetRot = Quaternion.Euler(dhPosition.localRot);
        }
        else
        {
            targetPos = origLocalPos;
            targetRot = Quaternion.Euler(new Vector3(0, 0, 0));
        }
        playerBodyData.body.localPosition = Vector3.Lerp(playerBodyData.body.localPosition, targetPos, lerpSpeed);
        playerBodyData.body.localRotation = Quaternion.Lerp(playerBodyData.body.localRotation, targetRot, lerpSpeed);
    }

    void HandleWallRunningAnimations(Animator bodyAnim, Animator tpAnim, Animator weaponIKAnim, bool wrLeft, bool wrRight)
    {
        //Tell both animators that we are wall-running
        bodyAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        bodyAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
        tpAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        tpAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
    }
}