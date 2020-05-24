using System;
using UnityEngine;

public class Sliding : IState
{
    public bool IsSliding { get; private set; } = false;
    
    private readonly Player _player;
    private readonly CharacterController _characterController;
    private readonly Transform _playerBody;
    private readonly Transform _playerCamera;
    private readonly float _originalCharacterHeight;
    private readonly float _originalCameraHeight;

    private bool _lowering = false;

    private const float DragAmount = 0.01f;
    private const float CrouchThreshold = 1f;

    public Sliding(Player player)
    {
        _player = player;
        _playerBody = player.PlayerBody;
        _playerCamera = player.PlayerCamera.transform;
        _characterController = player.GetComponent<CharacterController>();
        
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
        }
        stateParams.Velocity = velocity;
        return stateParams;
    }
    
    private void Crouch()
    {
        var targetBodyScale = 0.5f;
        var targetCcHeight = _originalCharacterHeight / 2;
        var targetCameraHeight = _originalCameraHeight / 2;
        
        var playerBodyScale = _playerBody.localScale;
        var ccHeight = _characterController.height;
        var ccCenter = _characterController.center;
        var cameraPosition = _playerCamera.transform.localPosition;

        playerBodyScale.y = Lower(playerBodyScale.y, targetBodyScale, Time.deltaTime * 8f);
        ccHeight = Lower(ccHeight, targetCcHeight, Time.deltaTime * 4f);
        ccCenter = Vector3.down * (2f - ccHeight) / 2.0f;
        cameraPosition.y = Lower(cameraPosition.y, targetCameraHeight, Time.deltaTime * 8f);
        
        _playerBody.localScale = playerBodyScale;
        _characterController.height = ccHeight;
        _characterController.center = ccCenter;
        _playerCamera.transform.localPosition = cameraPosition;
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
        IsSliding = true;
        _lowering = true;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        IsSliding = false;
        _lowering = false;
        return stateParams;
    }
}