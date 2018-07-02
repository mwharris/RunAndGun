using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ControlAnimations : AbstractBehavior
{
    public Animator anim;

    private bool isForwardPressed = false;
    private bool isBackwardPressed = false;
    private bool isLeftPressed = false;
    private bool isRightPressed = false;
    private bool isAimPressed = false;
    private bool isJumpPressed = false;

    private void setInputVars()
    {
        isForwardPressed  = inputState.GetButtonPressed(inputs[0]);
        isBackwardPressed = inputState.GetButtonPressed(inputs[1]);
        isLeftPressed     = inputState.GetButtonPressed(inputs[2]);
        isRightPressed    = inputState.GetButtonPressed(inputs[3]);
        isAimPressed      = inputState.GetButtonPressed(inputs[4]);
        isJumpPressed     = inputState.GetButtonPressed(inputs[5]);
    }

    void Update()
    {
        setInputVars();

        if (isForwardPressed)
            anim.SetBool("WalkForward", true);
        else                  
            anim.SetBool("WalkForward", false);

        if (isBackwardPressed)
            anim.SetBool("WalkBackward", true);
        else
            anim.SetBool("WalkBackward", false);

        if (isLeftPressed)
            anim.SetBool("WalkLeft", true);
        else
            anim.SetBool("WalkLeft", false);

        if (isRightPressed)
            anim.SetBool("WalkRight", true);
        else
            anim.SetBool("WalkRight", false);

        if (inputState.playerIsSprinting)
            anim.SetBool("Sprinting", true);
        else
            anim.SetBool("Sprinting", false);
        
        if (isAimPressed)
            anim.SetBool("Aiming", true);
        else
            anim.SetBool("Aiming", false);

        if (inputState.playerIsCrouching)
            anim.SetBool("Crouching", true);
        else
            anim.SetBool("Crouching", false);

        anim.SetBool("Jumping", !inputState.playerIsGrounded);
        anim.SetFloat("JumpSpeed", inputState.playerVelocity.y);

        var fwdSpeed = Vector3.Dot(inputState.playerVelocity, transform.forward);
        var sideSpeed = Vector3.Dot(inputState.playerVelocity, transform.right);
        anim.SetFloat("ForwardSpeed", fwdSpeed);
        anim.SetFloat("SideSpeed", sideSpeed);
        Debug.Log("Forward: " + fwdSpeed);
        Debug.Log("SideSpeed: " + sideSpeed);
    }
    


}
