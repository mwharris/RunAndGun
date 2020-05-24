using System;
using System.Collections;
using UnityEngine;

public class Crouching : IState
{
    public bool IsCrouching { get; private set; } = false;
    
    private readonly Player _player;
    private readonly CharacterController _characterController;
    private readonly CapsuleCollider _shotCollider;
    private readonly Transform _playerBody;
    private readonly Transform _playerCamera;
    
    // 1.7 m/s
    private float _crouchingWalkSpeed = 1.7f;
    
    private readonly float _originalCharacterHeight;
    private readonly float _originalCameraHeight;
    private readonly float _crouchCharacterHeight = 1.12f;
    private readonly float _crouchCameraHeight = 1.5f;
    
    private bool _firstFrame = false;
    private bool _lowering = false;
    private bool _rising = false;
    private bool _toSprint = false;

    public bool Rising => _rising;
    public bool ToSprint => _toSprint;

    public Crouching(Player player)
    {
        _player = player;
        _playerBody = player.PlayerBody;
        _playerCamera = player.PlayerCamera.transform;
        _characterController = player.GetComponent<CharacterController>();
        _shotCollider = player.GetComponent<CapsuleCollider>();

        _originalCharacterHeight = _characterController.height;
        _originalCameraHeight = _playerCamera.transform.localPosition.y;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        HandleInput();
        CrouchOrStand();
        return HandleMovement(stateParams);
    }

    private void HandleInput()
    {
        // Toggle between crouching and rising when crouch is pressed
        if (!_firstFrame && PlayerInput.Instance.CrouchDown)
        {
            _lowering = !_lowering;
            _rising = !_rising;
        }
        else
        {
            _firstFrame = false;
        }
        // Sprint directly out of Crouch
        if (PlayerInput.Instance.ShiftDown)
        {
            _toSprint = true;
            _lowering = false;
            _rising = true;
        }
    }

    private void CrouchOrStand()
    {
        if (_lowering)
        {
            Crouch();
        }
        else if (_rising)
        {
            Stand();   
        }
    }
    
    private IStateParams HandleMovement(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;

        // Gather our vertical and horizontal input
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;

        // Apply these values to our player
        var tempVelocity = (_player.transform.forward * forwardSpeed) + (_player.transform.right * sideSpeed);
        tempVelocity *= _crouchingWalkSpeed;
        
        // Make sure we're never moving faster than our walking speed
        tempVelocity = Vector3.ClampMagnitude(tempVelocity, _crouchingWalkSpeed);
        
        // Update our stateParams velocity
        stateParamsVelocity.x = tempVelocity.x;
        stateParamsVelocity.z = tempVelocity.z;
        stateParams.Velocity = stateParamsVelocity;
        
        return stateParams;
    }
    
    private void Crouch()
    {
        IsCrouching = true;
        float lowerSpeed = Time.deltaTime * 10f;
        var targetCcHeight = _crouchCharacterHeight;
        var targetCameraHeight = _crouchCameraHeight;
        
        var ccHeight = _characterController.height;
        var ccCenter = _characterController.center;
        var cameraPosition = _playerCamera.transform.localPosition;
        var capsuleHeight = _shotCollider.height;
        var capsuleCenter = _shotCollider.center;

        ccHeight = Lower(ccHeight, targetCcHeight, lowerSpeed);
        capsuleHeight = ccHeight;
        // CC Radius
        ccCenter.y = (ccHeight / 2) + 0.3f;
        capsuleCenter.y = (capsuleHeight / 2) + 0.3f;
        cameraPosition.y = Lower(cameraPosition.y, targetCameraHeight, lowerSpeed);
        // Body Z
        // Head Z
        // Head Y
        
        _playerCamera.transform.localPosition = cameraPosition;
        _characterController.height = ccHeight;
        _characterController.center = ccCenter;
        _shotCollider.height = capsuleHeight;
        _shotCollider.center = capsuleCenter;
    }

    private void Stand()
    {
        IsCrouching = false;
        float riseSpeed = Time.deltaTime * (_toSprint ? 16f : 10f);
        var targetCcHeight = _originalCharacterHeight;
        var targetCameraHeight = _originalCameraHeight;
        
        var ccHeight = _characterController.height;
        var ccCenter = _characterController.center;
        var cameraPosition = _playerCamera.transform.localPosition;
        var capsuleHeight = _shotCollider.height;
        var capsuleCenter = _shotCollider.center;
        
        ccHeight = Raise(ccHeight, targetCcHeight, riseSpeed);
        capsuleHeight = ccHeight - 0.2f;
        ccCenter.y = (ccHeight / 2) + 0.3f;
        capsuleCenter.y = (capsuleHeight / 2) + 0.3f;
        cameraPosition.y = Raise(cameraPosition.y, targetCameraHeight, riseSpeed);
        
        _playerCamera.transform.localPosition = cameraPosition;
        _characterController.height = ccHeight;
        _characterController.center = ccCenter;
        _shotCollider.height = capsuleHeight;
        _shotCollider.center = capsuleCenter;

        _rising = !FinishedStanding(targetCcHeight, targetCameraHeight);
    }

    private bool FinishedStanding(float targetCcHeight, float targetCameraHeight)
    {
        if (_characterController.height == targetCcHeight
            && _playerCamera.transform.localPosition.y == targetCameraHeight)
        {
            return true;
        }
        return false;
    }
    
    private float Lower(float value, float target, float deltaTime)
    {
        if (value > target && Mathf.Abs(value - target) > 0.01f)
        {
            return Mathf.Lerp(value, target, deltaTime);
        }
        else
        {
            _lowering = false;
            return target;
        }
    }

    private float Raise(float value, float target, float deltaTime)
    {
        if (value < target && Mathf.Abs(value - target) > 0.05f)
        {
            return Mathf.Lerp(value, target, deltaTime);
        }
        else
        {
            _rising = false;
            return target;
        }
    }
    
    public IStateParams OnEnter(IStateParams stateParams)
    {
        _lowering = true;
        _firstFrame = true;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        _toSprint = false;
        return stateParams;
    }

}