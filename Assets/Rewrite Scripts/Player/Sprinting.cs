using UnityEngine;

public class Sprinting : IState
{
    private readonly Player _player;
    private readonly InputState _inputState;
    private readonly Buttons[] _inputs;
    
    // 5.1 m/s
    private float _sprintingSpeed = 10.2f;

    public Sprinting(InputState inputState, Buttons[] inputs, Player player)
    {
        _player = player;
        _inputState = inputState;
        _inputs = inputs;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;

        // Gather our vertical and horizontal input
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;

        // Apply these values to our player
        var tempVelocity = (_player.transform.forward * forwardSpeed) + (_player.transform.right * sideSpeed);
        tempVelocity *= _sprintingSpeed;
            
        // Make sure we're never moving faster than our walking speed
        tempVelocity = Vector3.ClampMagnitude(tempVelocity, _sprintingSpeed);
            
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
        return stateParams;
    }

    public IStateParams OnExit(IStateParams stateParams)
    {
        return stateParams;
    }
}