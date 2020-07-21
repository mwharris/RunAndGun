using UnityEngine;

public class Sprinting : IState
{
    private readonly Player _player;
    private readonly CameraController _cameraController;
    private AudioController _audioController;
    
    private float _sprintingSpeed = 10.2f;    // 5.1 m/s
    private float _sprintTimer = 0.25f;
    private readonly float _origSprintTimer;

    public Sprinting(Player player, CameraController cameraController, AudioController audioController)
    {
        _player = player;
        _cameraController = cameraController;
        _audioController = audioController;
        _origSprintTimer = _sprintTimer;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        _sprintTimer -= Time.deltaTime;
        
        var stateParamsVelocity = stateParams.Velocity;

        // Gather our vertical and horizontal input
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;

        // Apply these values to our player
        var tempVelocity = (_player.transform.forward * forwardSpeed) + (_player.transform.right * sideSpeed);
        tempVelocity *= _sprintingSpeed;
            
        // Make sure we're never moving faster than our walking speed
        tempVelocity = Vector3.ClampMagnitude(tempVelocity, _sprintingSpeed);
        
        // Play footstep audio
        _sprintTimer = _audioController.PlayFootstep(_sprintTimer, _origSprintTimer, forwardSpeed, sideSpeed);
            
        // Update our stateParams velocity
        stateParamsVelocity.x = tempVelocity.x;
        stateParamsVelocity.z = tempVelocity.z;
        stateParams.Velocity = stateParamsVelocity;
            
        return stateParams;
    }

    // Decide when we are no longer sprinting
    public bool IsStillSprinting()
    {
        // If we hit the shift button again, turn off sprint
        if (PlayerInput.Instance.ShiftDown)
        {
            return false;
        }
        // If we stop moving forward, turn off sprint
        if (PlayerInput.Instance.Vertical <= 0)
        {
            return false;
        }
        return true;
    }

    public IStateParams OnEnter(IStateParams stateParams)
    {
        _cameraController.PlayerIsSprinting = true;
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        _cameraController.PlayerIsSprinting = false;
        return stateParams;
    }
}