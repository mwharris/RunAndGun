using System;
using UnityEngine;

public class Sliding : IState
{
    public bool IsSliding { get; private set; } = false;
    public bool NoIdle { get; private set; } = false;
    private bool NoStand { get; set; } = false;

    private readonly Player _player;
    private readonly Transform _playerCamera;
    private readonly CharacterController _characterController;
    private readonly CapsuleCollider _shotCollider;
    private readonly CameraController _cameraController;
    private readonly float _originalCharacterHeight;
    private readonly float _originalCameraHeight;

    private bool _lowering = false;

    private const float CrouchThreshold = 1f;
    private const float CrouchCharacterHeight = 1.12f;
    private const float CrouchCameraHeight = 1.5f;
    
    public Sliding(Player player)
    {
        _player = player;
        _playerCamera = player.PlayerCamera.transform;
        _characterController = player.GetComponent<CharacterController>();
        _shotCollider = player.GetComponent<CapsuleCollider>();
        _cameraController = player.GetComponent<CameraController>();
        _originalCharacterHeight = _characterController.height;
        _originalCameraHeight = _playerCamera.transform.localPosition.y;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        var velocity = stateParams.Velocity;
        
        // Lower into a crouch if we aren't lowered already
        if (_lowering)
        {
            Crouch();
        }
        
        // Apply drag to our velocity
        velocity.x *= 1 - Time.deltaTime;
        velocity.z *= 1 - Time.deltaTime;
        
        // Transition to Crouched state when velocity is under a threshold
        var horizontalVelocity = new Vector3(velocity.x, 0, velocity.z);
        if (horizontalVelocity.magnitude < CrouchThreshold)
        {
            IsSliding = false;
            NoStand = true;
        }

        stateParams.Velocity = velocity;
        return stateParams;
    }

    private void Crouch()
    {
        float lowerSpeed = Time.deltaTime * 10f;
        var targetCcHeight = CrouchCharacterHeight;
        var targetCameraHeight = CrouchCameraHeight;

        var ccHeight = _characterController.height;
        var ccCenter = _characterController.center;
        var cameraPosition = _playerCamera.transform.localPosition;
        var capsuleHeight = _shotCollider.height;
        var capsuleCenter = _shotCollider.center;

        ccHeight = Lower(ccHeight, targetCcHeight, lowerSpeed);
        capsuleHeight = ccHeight;
        ccCenter.y = (ccHeight / 2) + 0.3f;
        capsuleCenter.y = (capsuleHeight / 2) + 0.3f;
        cameraPosition.y = Lower(cameraPosition.y, targetCameraHeight, lowerSpeed);

        _playerCamera.transform.localPosition = cameraPosition;
        _characterController.height = ccHeight;
        _characterController.center = ccCenter;
        _shotCollider.height = capsuleHeight;
        _shotCollider.center = capsuleCenter;
    }

    private void Stand()
    {
        var targetCcHeight = _originalCharacterHeight;
        var targetCameraHeight = _originalCameraHeight;

        var ccHeight = _characterController.height;
        var ccCenter = _characterController.center;
        var cameraPosition = _playerCamera.transform.localPosition;
        var capsuleHeight = _shotCollider.height;
        var capsuleCenter = _shotCollider.center;

        ccHeight = targetCcHeight;
        capsuleHeight = ccHeight - 0.2f;
        ccCenter.y = (ccHeight / 2) + 0.3f;
        capsuleCenter.y = (capsuleHeight / 2) + 0.3f;
        cameraPosition.y = targetCameraHeight;

        _playerCamera.transform.localPosition = cameraPosition;
        _characterController.height = ccHeight;
        _characterController.center = ccCenter;
        _shotCollider.height = capsuleHeight;
        _shotCollider.center = capsuleCenter;
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

    public IStateParams OnEnter(IStateParams stateParams)
    {
        _cameraController.PlayerIsSliding = true;
        _lowering = true;
        IsSliding = true;
        NoIdle = true;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        _cameraController.PlayerIsSliding = false;
        _lowering = false;
        IsSliding = false;
        NoIdle = false;
        // Don't stand up if we're moving to crouched
        if (!NoStand)
        {
            Stand();
        }
        else
        {
            NoStand = false;
        }
        return stateParams;
    }
}