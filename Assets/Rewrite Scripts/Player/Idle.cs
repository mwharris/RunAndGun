using System;
using UnityEngine;

public class Idle : IState
{
    private readonly CharacterController _characterController;
    private readonly InputState _inputState;
    private readonly Buttons[] _inputs;
    
    public Idle(InputState inputState, Buttons[] inputs, Player player)
    {
        _characterController = player.GetComponent<CharacterController>();
        _inputState = inputState;
        _inputs = inputs;
    }

    public IStateParams Tick(IStateParams stateParams)
    {
        var stateParamsVelocity = stateParams.Velocity;
        stateParamsVelocity.x = 0;
        stateParamsVelocity.z = 0;
        stateParams.Velocity = stateParamsVelocity;
        return stateParams;
    }
    
    public bool IsIdle()
    {
        var noHorizontal = !PlayerInput.Instance.HorizontalHeld;
        var noVertical = !PlayerInput.Instance.VerticalHeld;
        return noHorizontal && noVertical && _characterController.isGrounded;
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