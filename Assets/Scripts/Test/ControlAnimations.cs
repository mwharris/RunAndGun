using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Animator anim;
    
    private bool isAimPressed = false;

    private void setInputVars()
    {
        isAimPressed = inputState.GetButtonPressed(inputs[0]);
    }

    void Update()
    {
        setInputVars();
        
        anim.SetBool("Sprinting", inputState.playerIsSprinting);
        
        anim.SetBool("Aiming", isAimPressed);

        anim.SetBool("Crouching", inputState.playerIsCrouching);

        anim.SetBool("Shooting", inputState.playerIsShooting);

        anim.SetBool("Jumping", !inputState.playerIsGrounded);
        anim.SetFloat("JumpSpeed", inputState.playerVelocity.y);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        anim.SetFloat("ForwardSpeed", fwdSpeed);
        anim.SetFloat("SideSpeed", sideSpeed);

        anim.SetFloat("LookAngle", inputState.playerLookAngle);
    }
}
