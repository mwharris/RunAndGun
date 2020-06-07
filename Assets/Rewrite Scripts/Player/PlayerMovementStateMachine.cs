using System;
using UnityEngine;

public class PlayerMovementStateMachine : MonoBehaviour
{
    private BaseStateMachine _stateMachine;
    private IStateParams _stateParams;
    private PlayerMovementStateMachineHelper _stateHelper;
    private WallRunHelper _wallRunHelper;
    private CharacterController _characterController;
    private CameraController _cameraController;
    private ControlAnimations _animationController;
    private PlayerLookVars _playerLookVars;
    
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _horizontalVelocity = Vector3.zero;
    private float defaultGravity = -14f;
    private bool _isWallRunning = false;
    
    public Type CurrentStateType => _stateMachine.CurrentState.GetType();
    public bool IsGrounded => _characterController.isGrounded;
    public Vector3 PlayerVelocity => _stateParams.Velocity;

    [SerializeField] private bool playerIsGrounded;

    private void Awake()
    {
        Player player = FindObjectOfType<Player>();
        _characterController = GetComponent<CharacterController>();
        _cameraController = GetComponent<CameraController>();
        _animationController = GetComponent<ControlAnimations>();
        _stateHelper = new PlayerMovementStateMachineHelper();
        _stateMachine = new BaseStateMachine();
        _playerLookVars = new PlayerLookVars();
        _wallRunHelper = new WallRunHelper();

        // Hook into the BaseStateMachine OnStateChanged event
        _stateMachine.OnStateChanged += HandleStateChanged;
        
        // Prepare our StateParams for passing to all of our states
        _stateParams = new StateParams();
        _stateParams.Velocity = _velocity;
        _stateParams.GravityOverride = defaultGravity;

        // Create our states
        Idle idle = new Idle(player);
        Walking walking = new Walking(player);
        Sprinting sprinting = new Sprinting(player, _cameraController);
        Jumping jumping = new Jumping(player, _cameraController, _animationController);
        WallRunning wallRunning = new WallRunning(player, defaultGravity, _cameraController);
        Crouching crouching = new Crouching(player);
        Sliding sliding = new Sliding(player);

        // Create our state transitions
        // Any -> Idle
        _stateMachine.AddAnyTransition(idle, () => _stateHelper.ToIdle(idle, jumping, crouching, sliding));
        // Any -> Jumping
        _stateMachine.AddAnyTransition(jumping,
            () => _stateHelper.ToJump(jumping, _isWallRunning, _stateParams.WallJumped));

        // Idle -> Walking
        _stateMachine.AddTransition(idle, walking, () => walking.IsWalking());
        // Walking -> Sprinting
        _stateMachine.AddTransition(walking, sprinting, () => PlayerInput.Instance.ShiftDown);
        // Sprinting -> Walking
        _stateMachine.AddTransition(sprinting, walking, () => !sprinting.IsStillSprinting());

        // Idle -> Crouching
        _stateMachine.AddTransition(idle, crouching, () => PlayerInput.Instance.CrouchDown);
        // Walking -> Crouching
        _stateMachine.AddTransition(walking, crouching, () => PlayerInput.Instance.CrouchDown);
        // Crouching -> Walking
        _stateMachine.AddTransition(crouching, walking, () => _stateHelper.CrouchToWalk(crouching, walking));
        // Crouching -> Sprinting
        _stateMachine.AddTransition(crouching, sprinting, () => _stateHelper.CrouchToSprint(crouching));
        // Sprinting -> Sliding (Crouching)
        _stateMachine.AddTransition(sprinting, sliding, () => PlayerInput.Instance.CrouchDown);

        // Sliding -> Crouching
        _stateMachine.AddTransition(sliding, crouching, () => !sliding.IsSliding);
        
        // Jumping -> Sliding
        _stateMachine.AddTransition(jumping, sliding, () => _stateHelper.JumpToSlide(jumping));
        // Jumping -> Sprinting
        _stateMachine.AddTransition(jumping, sprinting,
            () => _stateHelper.JumpToSprint(jumping, _stateParams.PreserveSprint));
        // Jumping -> Walking
        _stateMachine.AddTransition(jumping, walking,
            () => _stateHelper.JumpToWalk(jumping, walking, _stateParams.PreserveSprint));
        // Jumping -> Wall Running
        _stateMachine.AddTransition(jumping, wallRunning, () => _isWallRunning);

        // Wall Running -> Sprinting
        _stateMachine.AddTransition(wallRunning, jumping,
            () => _stateHelper.WallRunToSprint(jumping, _isWallRunning, _stateParams.PreserveSprint));
        // Wall Running -> Walking
        _stateMachine.AddTransition(wallRunning, jumping,
            () => _stateHelper.WallRunToWalk(jumping, walking, _isWallRunning));
        
        // Default to Idle
        _stateParams = _stateMachine.SetState(idle, _stateParams);
    }

    private void Update()
    {
        if (_characterController.isGrounded && _velocity.y < 0.1f)
        {
            _velocity.y = -2.5f;
        }

        playerIsGrounded = _characterController.isGrounded;
        
        // Wall-running raycast and hit info checks
        _isWallRunning = _wallRunHelper.DoWallRunCheck(_stateParams, transform, _velocity, _isWallRunning,
            _characterController.isGrounded);

        // Tick our current state to handle our movement
        _stateParams.Velocity = _velocity;
        _stateParams = _stateMachine.Tick(_stateParams);
        _velocity = _stateParams.Velocity;
        
        // Update our horizontal velocity variable
        _horizontalVelocity.x = _velocity.x;
        _horizontalVelocity.z = _velocity.z;

        // Handle our horizontal movement
        _characterController.Move(_velocity * Time.deltaTime);
        
        // Apply gravity (call move twice because t-squared)
        HandleGravity();

        // DebugPrintVelocity();
        DebugWallRunRaycast();
    }

    private void HandleGravity()
    {
        if (_stateMachine.CurrentState is WallRunning)
        {
            _velocity.y += _stateParams.GravityOverride * Time.deltaTime;
        }
        else if (_velocity.y > 0)
        {
            _velocity.y += defaultGravity * Time.deltaTime;
        }
        else
        {
            _velocity.y += (defaultGravity * 1.25f) * Time.deltaTime;
        }
        _characterController.Move(_velocity * Time.deltaTime);
    }
    
    private void HandleStateChanged(IState from, IState to)
    {
        if (from is Sprinting && to is Jumping)
        {
            _stateParams.PreserveSprint = true;
        }
        else if (_stateParams.PreserveSprint && from is Jumping && !(to is WallRunning))
        {
            _stateParams.PreserveSprint = false;
        }
        else if (from is WallRunning)
        {
            _isWallRunning = false;
        }
    }

    public PlayerLookVars GetPlayerLookVars()
    {
        _playerLookVars.PlayerIsWallRunning = CurrentStateType == typeof(WallRunning);
        _playerLookVars.WallRunZRotation = _stateParams.WallRunZRotation;
        return _playerLookVars;
    }

    private void DebugPrintVelocity()
    {
        Vector3 horizontalVelocity = new Vector3(_velocity.x, 0f, _velocity.z); 
        Debug.Log(
            "Velocity: " + _velocity 
                         + ", Horiz. Magnitude: " + horizontalVelocity.magnitude
                         /*+ ", Magnitude: " + _velocity.magnitude*/
        );
    }

    private void DebugWallRunRaycast()
    {
        Vector3 vDir = new Vector3(_velocity.x, 0, _velocity.z);
        Debug.DrawRay(transform.position, vDir, Color.yellow);

        Vector3 iDir = _wallRunHelper.CreateInputVector(transform);
        Debug.DrawRay(transform.position, iDir, Color.red);

        Vector3 rDir = transform.right;
        Vector3 lDir = -transform.right;
        Debug.DrawRay(transform.position, rDir, Color.white);
        Debug.DrawRay(transform.position, lDir, Color.white);
    }
}