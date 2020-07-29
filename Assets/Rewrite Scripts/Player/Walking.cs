using UnityEngine;

public class Walking : IState
{
    private readonly Player _player;
    private readonly CharacterController _characterController;
    private AudioController _audioController;
    
    private float _walkingSpeed = 6.8f;    // 3.4 m/s
    private float _aimWalkSpeed = 3.4f; // 1.7 m/s
    private float _walkTimer = 0.4f;
    private readonly float _origWalkTimer;

    public Walking(Player player, AudioController audioController)
    {
        _player = player;
        _characterController = player.GetComponent<CharacterController>();
        _audioController = audioController;
        _origWalkTimer = _walkTimer;
    }
    
    public IStateParams Tick(IStateParams stateParams)
    {
        _walkTimer -= Time.deltaTime;
        
        var stateParamsVelocity = stateParams.Velocity;

        // Gather our vertical and horizontal input
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;

        // Apply these values to our player
        var tempVelocity = (_player.transform.forward * forwardSpeed) + (_player.transform.right * sideSpeed);
        tempVelocity *= PlayerInput.Instance.AimHeld ? _aimWalkSpeed : _walkingSpeed;
        
        // Make sure we're never moving faster than our walking speed
        tempVelocity = Vector3.ClampMagnitude(tempVelocity, _walkingSpeed);
        
        // Play footstep audio
        _walkTimer = _audioController.PlayFootstep(_walkTimer, _origWalkTimer, forwardSpeed, sideSpeed);

        // Update our stateParams velocity
        stateParamsVelocity.x = tempVelocity.x;
        stateParamsVelocity.z = tempVelocity.z;
        stateParams.Velocity = stateParamsVelocity;
        
        return stateParams;
    }
    
    public bool IsWalking()
    {
        var inputHeld = PlayerInput.Instance.HorizontalHeld || PlayerInput.Instance.VerticalHeld;
        return _characterController.isGrounded && inputHeld;
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