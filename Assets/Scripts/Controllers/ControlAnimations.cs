using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public AnimationPosInfo crouchPosition;
    public AnimationPosInfo dhPosition;
    public bool JumpStart { get; set; } = false;

    private BodyController _bodyController;
    private PlayerBodyData _playerBodyData;
    private PlayerMovementStateMachine _playerMovementStateMachine;

    void Start()
    {
        _playerMovementStateMachine = GetComponent<PlayerMovementStateMachine>();
        //PlayerBodyData stores all info needed to control either our 1st or 3rd person body
        _bodyController = GetComponent<BodyController>();
        _playerBodyData = _bodyController.PlayerBodyData;
    }

    void Update()
    {
        _playerBodyData = _bodyController.PlayerBodyData;
        WeaponData currWeaponData = _playerBodyData.GetWeaponData();

        //Alias player body data information for later
        Animator bodyAnim = _playerBodyData.GetBodyAnimator();
        //Animator weaponIKAnim = currWeaponData.WeaponIKAnimator;
        Animator weaponAnim = _playerBodyData.weapon.GetComponent<Animator>();
        Animator thirdPersonAnim = _bodyController.ThirdPersonBody.GetBodyAnimator();
        
        SetAnimatorParams(bodyAnim);
        SetAnimatorParams(thirdPersonAnim);

        // TODO: clean everything underneath this up
        bodyAnim.SetBool("SingleHanded", inputState.playerWeaponStyle == WeaponStyles.SingleHanded);
        bodyAnim.SetBool("DoubleHanded", inputState.playerWeaponStyle == WeaponStyles.DoubleHanded);
        if (inputState.playerIsShooting)
        {
            weaponAnim.SetTrigger("ShootTrig");
        }
    }

    private void SetAnimatorParams(Animator anim)
    {
        var fwdSpeed = Vector3.Dot(_playerMovementStateMachine.PlayerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(_playerMovementStateMachine.PlayerVelocity, transform.right);
        var crouching = _playerMovementStateMachine.PlayerIsCrouching;
        var sliding = _playerMovementStateMachine.PlayerIsSliding;
        var sprinting = _playerMovementStateMachine.PlayerIsSprinting;
        
        anim.SetBool("Crouching", crouching || sliding);
        anim.SetBool("Sprinting", sprinting);
        anim.SetBool("Jumping", !_playerMovementStateMachine.PlayerIsGrounded);
        anim.SetFloat("JumpSpeed", _playerMovementStateMachine.PlayerVelocity.y);
        anim.SetBool("JumpStart", JumpStart);
        anim.SetFloat("ForwardSpeed", fwdSpeed);
        anim.SetFloat("SideSpeed", sideSpeed);
        anim.SetBool("WallRunningLeft", _playerMovementStateMachine.PlayerIsWallRunningLeft);
        anim.SetBool("WallRunningRight", _playerMovementStateMachine.PlayerIsWallRunningRight);
        
        // TODO: REMOVE INPUT STATE
        anim.SetBool("Aiming", inputState.playerIsAiming);
        anim.SetFloat("LookAngle", inputState.playerLookAngle);
        
        if (inputState.playerIsShooting)
        {
            anim.SetTrigger("ShootTrig");
        }

        if (inputState.playerIsReloading)
        {
            anim.SetTrigger("ReloadTrig");
        }
        else {
            anim.ResetTrigger("ReloadTrig");
        }
    }
}