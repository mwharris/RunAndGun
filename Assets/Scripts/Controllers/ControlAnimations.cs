using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Animator bodyAnim;
    public Animator weaponAnim;

    private bool isAimPressed = false;

    private void setInputVars()
    {
        isAimPressed = inputState.GetButtonPressed(inputs[0]);
    }

    void Update()
    {
        setInputVars();
        
        bodyAnim.SetBool("Sprinting", inputState.playerIsSprinting);
        
        bodyAnim.SetBool("Aiming", isAimPressed);
        weaponAnim.SetBool("Aiming", isAimPressed);

        bodyAnim.SetBool("Crouching", inputState.playerIsCrouching);

        bodyAnim.SetBool("Shooting", inputState.playerIsShooting);

        bodyAnim.SetBool("Reloading", inputState.playerIsReloading);

        bodyAnim.SetBool("Jumping", !inputState.playerIsGrounded);
        bodyAnim.SetFloat("JumpSpeed", inputState.playerVelocity.y);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        bodyAnim.SetFloat("ForwardSpeed", fwdSpeed);
        bodyAnim.SetFloat("SideSpeed", sideSpeed);

        bodyAnim.SetFloat("LookAngle", inputState.playerLookAngle);
    }
}
