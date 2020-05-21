using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovementStateMachineHelper
{
    private bool _fixInitialNotGrounded = true;

    public bool ToIdle(Idle idle, Jumping jumping, Crouching crouching, Sliding sliding)
    {
        return idle.IsIdle()
               && !jumping.IsJumping()
               && (!crouching.IsCrouching && !crouching.Rising)
               && !sliding.IsSliding;
    }
    
    public bool ToJump(Jumping jumping, bool isWallRunning, bool isWallJumping)
    {
        bool wallJumped = !isWallRunning || isWallJumping;
        return jumping.IsJumping() 
               && wallJumped
               && !FixInitialNotGrounded();
    }
    
    private bool FixInitialNotGrounded()
    {
        if (_fixInitialNotGrounded)
        {
            _fixInitialNotGrounded = false;
            return true;
        } 
        return false;
    }

    public bool JumpToSlide(Jumping jumping)
    {
        return !jumping.IsJumping() && jumping.ToSlide;
    }
    
    public bool JumpToSprint(Jumping jumping, bool preserveSprint)
    {
        return !jumping.IsJumping() && !jumping.ToSlide && preserveSprint;
    }

    public bool JumpToWalk(Jumping jumping, Walking walking, bool preserveSprint)
    {
        return !jumping.IsJumping() && !jumping.ToSlide && !preserveSprint && walking.IsWalking();
    }

    public bool WallRunToSprint(Jumping jumping, bool isWallRunning, bool preserveSprint)
    {
        return !isWallRunning 
               && !jumping.IsJumping() 
               && preserveSprint;
    }

    public bool WallRunToWalk(Jumping jumping, Walking walking, bool isWallRunning)
    {
        return !isWallRunning 
               && !jumping.IsJumping() 
               && walking.IsWalking();
    }

    public bool CrouchToWalk(Crouching crouching, Walking walking)
    {
        return walking.IsWalking() && !crouching.ToSprint && !crouching.IsCrouching && !crouching.Rising;
    }

    public bool CrouchToSprint(Crouching crouching)
    {
        return crouching.ToSprint && !crouching.IsCrouching && !crouching.Rising;
    }
}
