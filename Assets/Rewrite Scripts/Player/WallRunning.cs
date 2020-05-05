using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WallRunning : IState
{
    private readonly Player _player;
    private readonly Transform _playerCamera;
    private readonly float _gravity;
    private readonly InputState _inputState;
    private readonly Buttons[] _inputs;
    
    private readonly float _wallRunSpeed = 6.8f;
    private readonly float _wallRunSlowSpeed = 1f;
    private readonly float _wallRunCameraTilt = 0.08f;
    
    private Vector3 _wallRunMoveAxis = Vector3.zero;
    private bool _wallRunningRight = false;
    private bool _wallRunningLeft = false;

    public WallRunning(InputState inputState, Buttons[] inputs, Player player, float defaultGravity)
    {
        _inputState = inputState;
        _inputs = inputs;
        _player = player;
        _playerCamera = player.PlayerCamera.transform;
        _gravity = defaultGravity;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;
        var wallRunHitInfo = stateParams.WallRunHitInfo;

        var forwardSpeed = PlayerInput.Instance.Vertical;
        bool wallJumped = PlayerInput.Instance.SpaceDown;

        if (wallJumped)
        {
            stateParams.WallJumped = true;
        }
        else
        {
            // Find the direction parallel to the wall using the wallRunHitInfo.normal
            SetWallRunSide();
            // Tilt the camera in the opposite direction of the wall-run
            TiltCamera(stateParams);
            // Wall running right
            if (_wallRunningRight)
            {
                _wallRunMoveAxis = Vector3.Cross(Vector3.up, wallRunHitInfo.normal);
            }
            // Wall running left
            else
            {
                _wallRunMoveAxis = Vector3.Cross(wallRunHitInfo.normal, Vector3.up);
            }
            // Apply our movement along the wall run axis we found above
            var moveAxis = _wallRunMoveAxis;
            moveAxis = (moveAxis * forwardSpeed);
            moveAxis *= _wallRunSpeed;
            moveAxis = Vector3.ClampMagnitude(moveAxis, _wallRunSpeed);
            // Update our stateParams velocity
            stateParamsVelocity.x = moveAxis.x;
            stateParamsVelocity.z = moveAxis.z;
            stateParams.Velocity = stateParamsVelocity;   
        }
        
        return SetGravity(stateParams);
    }

    private void TiltCamera(IStateParams stateParams)
    {
        var lerpSpeed = Time.deltaTime * 4f;
        // This value will be retrieved by the PlayerLook script via PlayerLookVars
        if (_wallRunningRight)
        {
            stateParams.WallRunZRotation = Mathf.Lerp(_playerCamera.localRotation.z, _wallRunCameraTilt, lerpSpeed);
        }
        else if (_wallRunningLeft) 
        {
            stateParams.WallRunZRotation = Mathf.Lerp(_playerCamera.localRotation.z, -_wallRunCameraTilt, lerpSpeed);
        }
    }

    private void SetWallRunSide()
    {
        float rayDistance = 1f;
        RaycastHit rightHitInfo;
        RaycastHit leftHitInfo;
        
        Vector3 rightDir = _player.transform.right;
        Vector3 leftDir = -_player.transform.right;
        
        Physics.Raycast(_player.transform.position, rightDir, out rightHitInfo, rayDistance);
        Physics.Raycast(_player.transform.position, leftDir, out leftHitInfo, rayDistance);

        _wallRunningRight = rightHitInfo.collider != null;
        _wallRunningLeft = leftHitInfo.collider != null;
    }

    private IStateParams SetGravity(IStateParams stateParams)
    {
        if (stateParams.Velocity.y < 0f)
        {
            stateParams.GravityOverride = _gravity / 4f;
        }
        else
        {
            stateParams.GravityOverride = _gravity / 1.5f;
        }
        return stateParams;
    }

    public IStateParams OnEnter(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;
        if (stateParamsVelocity.y < 0)
        {
            stateParamsVelocity.y = 0;
            stateParams.Velocity = stateParamsVelocity;
        }
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        return stateParams;
    }
}