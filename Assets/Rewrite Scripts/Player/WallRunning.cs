using System;
using System.Diagnostics;
using UnityEngine;
using Debug = UnityEngine.Debug;

public class WallRunning : IState
{
    private readonly Player _player;
    private readonly Transform _playerCamera;
    private readonly float _gravity;
    private CameraController _cameraController;
    
    // 6.8 m/s
    private readonly float _wallRunSpeed = 13.6f;
    private readonly float _wallRunSlowSpeed = 1f;
    private readonly float _wallRunCameraTilt = 0.08f;
    
    private Vector3 _wallRunMoveAxis = Vector3.zero;

    public WallRunning(Player player, float defaultGravity, CameraController cameraController)
    {
        _player = player;
        _playerCamera = player.PlayerCamera.transform;
        _gravity = defaultGravity;
        _cameraController = cameraController;
    }

    // TODO: THIS NEEDS TO BE ON THE SAME PAGE AS WallRunHelper.DoWallRunCheck()
    // SOMETIMES THIS FUNCTION CANNOT FIND A WALL-RUN SIDE DESPITE DoWallRunCheck() SAYING WE ARE WALL-RUNNING
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
            SetWallRunSide(stateParams);
            // Tilt the camera in the opposite direction of the wall-run
            TiltCamera(stateParams);
            // Wall running right
            if (stateParams.WallRunningRight)
            {
                _wallRunMoveAxis = Vector3.Cross(Vector3.up, wallRunHitInfo.normal);
            }
            // Wall running left
            else if (stateParams.WallRunningLeft)
            {
                _wallRunMoveAxis = Vector3.Cross(wallRunHitInfo.normal, Vector3.up);
            }
            else
            {
                Debug.Log("WallRunning: This shouldn't happen!");
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
        if (stateParams.WallRunningRight)
        {
            stateParams.WallRunZRotation = Mathf.Lerp(_playerCamera.localRotation.z, _wallRunCameraTilt, lerpSpeed);
        }
        else if (stateParams.WallRunningLeft) 
        {
            stateParams.WallRunZRotation = Mathf.Lerp(_playerCamera.localRotation.z, -_wallRunCameraTilt, lerpSpeed);
        }
    }

    private void SetWallRunSide(IStateParams stateParams)
    {
        if (!stateParams.WallRunningRight && !stateParams.WallRunningLeft)
        {
            RaycastHit rightHitInfo;
            RaycastHit leftHitInfo;
            int layerMask = ~(1 << 10);

            Vector3 rayPos = new Vector3(_player.transform.position.x, _player.transform.position.y + 1,
                _player.transform.position.z);
            float rayDistance = 1f;

            Vector3 rightDir = _player.transform.right;
            Vector3 leftDir = -_player.transform.right;

            Physics.Raycast(rayPos, rightDir, out rightHitInfo, rayDistance, layerMask);
            Physics.Raycast(rayPos, leftDir, out leftHitInfo, rayDistance, layerMask);

            stateParams.WallRunningRight = rightHitInfo.collider != null;
            stateParams.WallRunningLeft = leftHitInfo.collider != null;
        }
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
        _cameraController.PlayerIsWallRunning = true;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        stateParams.WallRunningRight = false;
        stateParams.WallRunningLeft = false;
        _cameraController.PlayerIsWallRunning = false;
        return stateParams;
    }
}