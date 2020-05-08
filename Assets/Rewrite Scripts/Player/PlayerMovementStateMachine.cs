using System;
using UnityEngine;

public class PlayerMovementStateMachine : AbstractBehavior
{
    private BaseStateMachine _stateMachine;
    private CharacterController _characterController;
    private IStateParams _stateParams;
    private PlayerMovementStateMachineHelper _stateHelper;
    private PlayerLookVars _playerLookVars;
    
    private Vector3 _velocity = Vector3.zero;
    private Vector3 _horizontalVelocity = Vector3.zero;
    private float defaultGravity = -14f;
    private bool _preserveSprint = false;
    
    private bool _isWallRunning = false;
    
    public Type CurrentStateType => _stateMachine.CurrentState.GetType();
    public bool IsGrounded => _characterController.isGrounded;

    [SerializeField] private bool playerIsGrounded;

    private void Awake()
    {
        Player player = FindObjectOfType<Player>();
        _characterController = GetComponent<CharacterController>();
        _stateHelper = new PlayerMovementStateMachineHelper();
        _stateMachine = new BaseStateMachine();
        _playerLookVars = new PlayerLookVars();

        // Hook into the BaseStateMachine OnStateChanged event
        _stateMachine.OnStateChanged += HandleStateChanged;
        
        // Prepare our StateParams for passing to all of our states
        _stateParams = new StateParams();
        _stateParams.Velocity = _velocity;
        _stateParams.GravityOverride = defaultGravity;

        // Create our states
        Idle idle = new Idle(this.inputState, this.inputs, player);
        Walking walking = new Walking(this.inputState, this.inputs, player);
        Sprinting sprinting = new Sprinting(this.inputState, this.inputs, player);
        Jumping jumping = new Jumping(this.inputState, this.inputs, player);
        WallRunning wallRunning = new WallRunning(this.inputState, this.inputs, player, defaultGravity);
        Crouching crouching = new Crouching(this.inputState, this.inputs, player);
        Sliding sliding = new Sliding(this.inputState, this.inputs, player);

        // Create our state transitions
        // Any -> Idle
        _stateMachine.AddAnyTransition(idle, () => _stateHelper.ToIdle(idle, jumping, crouching, sliding));
        // Any -> Jumping
        _stateMachine.AddAnyTransition(jumping, () => _stateHelper.ToJump(jumping, _isWallRunning, _stateParams.WallJumped));

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
        
        // Jumping -> Sliding
        _stateMachine.AddTransition(jumping, sliding, () => _stateHelper.JumpToSlide(jumping));
        // Jumping -> Sprinting
        _stateMachine.AddTransition(jumping, sprinting, () => _stateHelper.JumpToSprint(jumping, _preserveSprint));
        // Jumping -> Walking
        _stateMachine.AddTransition(jumping, walking, () => _stateHelper.JumpToWalk(jumping, walking, _preserveSprint));
        // Jumping -> Wall Running
        _stateMachine.AddTransition(jumping, wallRunning, () => _isWallRunning);
        
        // Wall Running -> Sprinting
        _stateMachine.AddTransition(wallRunning, jumping, () => _stateHelper.WallRunToSprint(jumping, _isWallRunning, _preserveSprint));
        // Wall Running -> Walking
        _stateMachine.AddTransition(wallRunning, jumping, () => _stateHelper.WallRunToWalk(jumping, walking, _isWallRunning));
        
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
        DoWallRunCheck(_stateParams, _velocity, _characterController.isGrounded);

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

        DebugPrintVelocity();
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
            _preserveSprint = true;
        }
        else if (_preserveSprint && from is Jumping && !(to is WallRunning))
        {
            _preserveSprint = false;
        }
        else if (from is WallRunning)
        {
            _isWallRunning = false;
        }
    }

    private void DoWallRunCheck(IStateParams stateParams, Vector3 velocity, bool isGrounded)
    {
        float rayDistance = 1f;
        
        if (!isGrounded)
        {
            var lastHitInfo = stateParams.WallRunHitInfo;
            // Check initialization of wall-running rules.
            // Right now this just mean checking if our velocity vector touches any walls.
            if (!_isWallRunning)
            {
                RaycastHit velocityHitInfo;
                RaycastHit inputHitInfo;
                Vector3 vDir = new Vector3(velocity.x, 0, velocity.z);
                Vector3 iDir = CreateInputVector();
                Physics.Raycast(transform.position, vDir, out velocityHitInfo, rayDistance);
                Physics.Raycast(transform.position, iDir, out inputHitInfo, rayDistance);
                Debug.DrawRay(transform.position, Vector3.ClampMagnitude(vDir, rayDistance), Color.yellow);
                Debug.DrawRay(transform.position, Vector3.ClampMagnitude(iDir, rayDistance), Color.black);
                if (velocityHitInfo.collider != null || inputHitInfo.collider != null)
                {
                    _isWallRunning = true;
                    stateParams.WallRunHitInfo = velocityHitInfo.collider != null ? velocityHitInfo : inputHitInfo;
                    return;
                }
            }
            // Check continuous wall-running rules.
            // Raycast along the last hit info's normal, in the reverse direction, to see if we're still on a wall.
            else if (lastHitInfo.collider != null)
            {
                RaycastHit wallNormalHitInfo;
                Vector3 rayDir = new Vector3(-lastHitInfo.normal.x, 0, -lastHitInfo.normal.z);
                Physics.Raycast(transform.position, rayDir, out wallNormalHitInfo, rayDistance);
                if (wallNormalHitInfo.collider != null)
                {
                    stateParams.WallRunHitInfo = wallNormalHitInfo;
                    return;
                }
            }
        }
        // If we reached this point we shouldn't be wall-running
        _isWallRunning = false;
        // This is here to make sure we're not creating new RaycastHits every frame
        if (stateParams.WallRunHitInfo.collider != null)
        {
            stateParams.WallRunHitInfo = new RaycastHit();
        }
    }

    private Vector3 CreateInputVector()
    {
        float forwardSpeed = PlayerInput.Instance.Vertical;
        float sideSpeed = PlayerInput.Instance.Horizontal;
        return ((transform.forward * forwardSpeed) + (transform.right * sideSpeed));
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
    }

}