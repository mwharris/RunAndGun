using System;
using UnityEngine;

public class Jumping : IState
{
    private readonly Player _player;
    private readonly CharacterController _characterController;
    private CameraController _cameraController;
    private readonly InputState _inputState;
    private readonly Buttons[] _inputs;

    private bool _doJump = false;
    private bool _doubleJumpAvailable = true;

    private const float JumpSpeed = 6f;
    private const float WallJumpHorizontalSpeed = 17f;    // 8.5 m/s
    
    private bool JumpDown => PlayerInput.Instance.SpaceDown;
    private bool JumpHeld => PlayerInput.Instance.SpaceHeld;

    public bool ToSlide { get; private set; } = false;

    public Jumping(Player player, CameraController cameraController)
    {
        _player = player;
        _characterController = player.GetComponent<CharacterController>();
        _cameraController = cameraController;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;
        _cameraController.PlayerIsAirborneFast = stateParams.WallJumped || stateParams.PreserveSprint;

        // Gather input and create an input vector from the values
        float forwardSpeed = PlayerInput.Instance.VerticalRaw;
        float sideSpeed = PlayerInput.Instance.HorizontalRaw;
        var inputVelocity = (_player.transform.forward * forwardSpeed) + (_player.transform.right * sideSpeed);
        
        // If we hit Crouch while Jumping, we want to land in a slide
        if (PlayerInput.Instance.CrouchDown)
        {
            ToSlide = true;
        }

        // Handle aerial movement and wall jumping
        if (stateParams.WallJumped)
        {
            _doJump = false;
            stateParams.WallJumped = false;
            stateParamsVelocity = HandleWallJumping(stateParamsVelocity, forwardSpeed, sideSpeed);
        }
        else
        {
            stateParamsVelocity = HandleJumping(stateParamsVelocity, inputVelocity);
            stateParamsVelocity = HandleMovement(stateParamsVelocity, inputVelocity);
        }

        stateParams.Velocity = stateParamsVelocity;
        return stateParams;
    }

    private Vector3 HandleMovement(Vector3 velocity, Vector3 inputVelocity)
    {
        // Create a vector from our current velocity ignoring the y-axis
        var tempVelocity = new Vector3(velocity.x, 0f, velocity.z);
        // Determine the angle (in degrees) between this direction and our velocity direction
        float angle = Vector3.Angle(tempVelocity, inputVelocity);
        // Move while in the air if it's not directly forward
        if (angle > 0)
        {
            // Clamp the magnitude so that we're never actually speeding up
            float startingMagnitude = tempVelocity.magnitude;
            tempVelocity += inputVelocity * 0.1f;
            tempVelocity = Vector3.ClampMagnitude(tempVelocity, startingMagnitude);
            // Apply these updates to our velocity
            velocity.x = tempVelocity.x;
            velocity.z = tempVelocity.z;
        }
        return velocity;
    }

    private Vector3 HandleJumping(Vector3 velocity, Vector3 inputVelocity)
    {
        // Jump when we first enter the jump state
        if (_doJump)
        {
            velocity.y = JumpSpeed;
            _doJump = false;
        }
        // Jump when we hit the ground if we're holding the jump button
        else if (_characterController.isGrounded && JumpHeld)
        {
            velocity.y = JumpSpeed;
            _doubleJumpAvailable = true;
        }
        // Jump if we have a double jump and we hit Jump button
        else if (_doubleJumpAvailable && JumpDown)
        {
            velocity = DoubleJump(velocity, inputVelocity);
        }
        return velocity; 
    }
    
    private Vector3 HandleWallJumping(Vector3 velocity, float forwardSpeed, float sideSpeed)
    {
        Vector3 targetDir = new Vector3(sideSpeed, 0f, forwardSpeed);
        velocity = _player.transform.rotation * targetDir;
        velocity = velocity.normalized * WallJumpHorizontalSpeed;
        velocity.y = JumpSpeed;
        return velocity;
    }

    private Vector3 DoubleJump(Vector3 velocity, Vector3 inputVelocity)
    {
        if (Math.Abs(inputVelocity.magnitude) > 0)
        {
            velocity.y = 0;
            // Find the angle, in radians, between our input direction and current direction
            float degrees = Vector3.Angle(velocity, inputVelocity);
            float radians = degrees * Mathf.Deg2Rad;
            // Rotate the current direction to match our input direction
            velocity = Vector3.RotateTowards(velocity, inputVelocity, radians, 0.0f);
        }
        velocity.y = JumpSpeed;
        _doubleJumpAvailable = false;
        return velocity;
    }

    public IStateParams OnEnter(IStateParams stateParams)
    {
        if (PlayerInput.Instance.SpaceDown || stateParams.WallJumped)
        {
            _doJump = true;
        }
        _cameraController.PlayerIsAirborneFast = stateParams.WallJumped || stateParams.PreserveSprint;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        _doubleJumpAvailable = true;
        ToSlide = false;
        _cameraController.PlayerIsAirborneFast = false;
        return stateParams;
    }

    public bool IsJumping()
    {
        // If we hit or are holding the Jump button
        if (JumpDown || JumpHeld)
        {
            return true;
        }
        // If we are still airborne
        else if (!_characterController.isGrounded)
        {
            return true;
        }
        return false;
    }
}