using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Vector3 crouchLocalPos = Vector3.zero;
    public Vector3 crouchLocalRot = Vector3.zero;
    private Vector3 origCrouchLocalPos;
    private Quaternion origCrouchLocalRot;

    public Vector3 aimingLocalPos = Vector3.zero;
    public Vector3 aimingLocalRot = Vector3.zero;
    private Vector3 origLocalPos;
    private Quaternion origLocalRot;

    private BodyController bodyControl;
    private PlayerBodyData playerBodyData;
    private PhotonView pView;

    public Animator otherAnim;

    void Start()
    {
        pView = GetComponent<PhotonView>();
        //PlayerBodyData stores all info needed to control either our 1st or 3rd person body
        bodyControl = GetComponent<BodyController>();
        playerBodyData = bodyControl.PlayerBodyData;
        //Set original player body positions and rotations
        origLocalPos = playerBodyData.body.localPosition;
        origLocalRot = playerBodyData.body.localRotation;
    }

    void Update()
    {
        playerBodyData = bodyControl.PlayerBodyData;

        //Alias player body data information for later
        Animator bodyAnim = playerBodyData.bodyAnimator;
        Animator weaponIKAnim = playerBodyData.weaponIKAnim;
        Animator weaponAnim = playerBodyData.weaponAnim;

        //Set various Animator properties to control the animators properly
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);
        otherAnim.SetBool("Sprinting", inputState.playerIsSprinting);

        HandleBodyPlacement(inputState.playerIsAiming);

        bodyAnim.SetBool("Aiming", inputState.playerIsAiming);
        otherAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponIKAnim.SetBool("Aiming", inputState.playerIsAiming);
        weaponAnim.SetBool("Aiming", inputState.playerIsAiming);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);
        otherAnim.SetBool("Crouching", inputState.playerIsCrouching);

        bodyAnim.SetBool("Shooting", inputState.playerIsShooting);
        otherAnim.SetBool("Shooting", inputState.playerIsShooting);
        weaponAnim.SetBool("Shooting", inputState.playerIsShooting);

        if (inputState.playerIsShooting)
        {
            bodyAnim.SetTrigger("ShootTrig");
            otherAnim.SetTrigger("ShootTrig");
        }

        if (inputState.playerIsReloading)
        {
            bodyAnim.SetTrigger("ReloadTrig");
            otherAnim.SetTrigger("ReloadTrig");
        }

        HandleWallRunningAnimations(bodyAnim, weaponIKAnim, inputState.playerIsWallRunningLeft, inputState.playerIsWallRunningRight);

        bodyAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        bodyAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);
        bodyAnim.SetBool("JumpStart", inputState.playerIsJumping);
        otherAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        otherAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);
        otherAnim.SetBool("JumpStart", inputState.playerIsJumping);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        bodyAnim.SetFloat("ForwardSpeed", fwdSpeed);
        bodyAnim.SetFloat("SideSpeed", sideSpeed);
        otherAnim.SetFloat("ForwardSpeed", fwdSpeed);
        otherAnim.SetFloat("SideSpeed", sideSpeed);

        bodyAnim.SetFloat("LookAngle", inputState.playerLookAngle);
        otherAnim.SetFloat("LookAngle", inputState.playerLookAngle);
    }

    void HandleBodyPlacement(bool isAiming)
    {
        float lerpSpeed = Time.deltaTime * 20f;
        Vector3 currPos = playerBodyData.body.localPosition;
        Quaternion currRot = playerBodyData.body.localRotation;
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
                currPos = Vector3.Lerp(currPos, crouchLocalPos, Time.deltaTime * 10f);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(crouchLocalRot), Time.deltaTime * 10f);
            }
        }
        else
        {
            currPos = Vector3.Lerp(currPos, origLocalPos, lerpSpeed);
            currRot = Quaternion.Lerp(currRot, Quaternion.Euler(new Vector3(0, 0, 0)), lerpSpeed);
        }
        playerBodyData.body.localPosition = currPos;
        playerBodyData.body.localRotation = currRot;
    }

    void HandleWallRunningAnimations(Animator bodyAnim, Animator weaponIKAnim, bool wrLeft, bool wrRight)
    {
        //Tell both animators that we are wall-running
        bodyAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        bodyAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
        otherAnim.SetBool("WallRunningLeft", inputState.playerIsWallRunningLeft);
        otherAnim.SetBool("WallRunningRight", inputState.playerIsWallRunningRight);
    }
}