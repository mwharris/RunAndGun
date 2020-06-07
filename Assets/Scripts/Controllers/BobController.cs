using UnityEngine;
using System.Collections;

public class BobController : AbstractBehavior
{
    [SerializeField] private Transform bobTarget;
    public bool isCamera = false;

    [SerializeField] private AnimationPosInfo restPosInfo;
    [SerializeField] private AnimationPosInfo sprintPosInfo;
    [SerializeField] private AnimationPosInfo shAimPosInfo;
    [SerializeField] private AnimationPosInfo dhAimPosInfo;
    [SerializeField] private AnimationPosInfo crouchPosInfo;
    private AnimationPosInfo _origRestPosInfo;

    [HideInInspector] public float transitionSpeed = 20f;
    [HideInInspector] public float walkBobSpeed = 7.8f;
    [HideInInspector] public float walkBobAmount = 0.01f;
    [HideInInspector] public float sprintBobSpeed = 12f;
    [HideInInspector] public float sprintBobAmount = 0.03f;

    private PlayerMovementStateMachine _playerMovementStateMachine;
    private bool _aiming = false;
    private bool _sprinting = false;
    private Vector3 _lerpToPos;
    private Quaternion _lerpToRot;

    // TODO: REMOVE INPUT STATE
    private bool PlayerIsAiming => inputState.playerIsAiming; 
    private bool PlayerIsGrounded => _playerMovementStateMachine.IsGrounded;
    private bool PlayerIsSprinting => _playerMovementStateMachine.CurrentStateType == typeof(Sprinting);
    private bool PlayerIsCrouching => _playerMovementStateMachine.CurrentStateType == typeof(Crouching);

    private float HorizontalRaw => PlayerInput.Instance.HorizontalRaw;
    private float VerticalRaw => PlayerInput.Instance.VerticalRaw;

    //Initialized as this value because this is where sin = 1. 
    //So, this will make the camera always start at the crest of the sin wave, simulating someone picking up their foot and starting to walk.
    //You experience a bob upwards when you start walking as your foot pushes off the ground, the left and right bobs come as you walk.
    private float _timer = Mathf.PI / 2;

    void Start()
    {
        _playerMovementStateMachine = GetComponent<PlayerMovementStateMachine>();
        if (bobTarget == null)
        {
            bobTarget = transform;
        }

        restPosInfo.localPos = bobTarget.localPosition;
        restPosInfo.rotation = bobTarget.localRotation;
        
        _origRestPosInfo = new AnimationPosInfo();
        _origRestPosInfo.localPos = restPosInfo.localPos;
        _origRestPosInfo.rotation = restPosInfo.rotation;
    }

    private bool reset = false;

    void Update()
    {
        bool singleHanded = inputState.playerWeaponStyle == WeaponStyles.SingleHanded;
        bool doubleHanded = inputState.playerWeaponStyle == WeaponStyles.DoubleHanded;
        //Determine if we need to reset our Rest Positions based on player state.
        SetRestPositionAndRotation(singleHanded, doubleHanded);
        //Call functions appropriate to the above function call
        if (reset)
        {
            HandleResetting();
        }
        else
        {
            HandleBob(doubleHanded);
            HandleTilt(doubleHanded);
        }
        //Completed a full cycle. Reset to 0 to avoid bloated values.
        if (_timer > Mathf.PI * 2)
        {
            _timer = 0;
        }
	}

    //Determine if we need to change the resting position of our cycle
    private void SetRestPositionAndRotation(bool singleHanded, bool doubleHanded)
    {
        //Only change positions if we're controlling the body
        if (isCamera) return;
        //When aiming change the resting position according to our current weapon
        if (PlayerIsAiming)
        {
            AnimationPosInfo posInfo = null;
            if (singleHanded)
            {
                posInfo = shAimPosInfo;
            }
            else if (doubleHanded) 
            {
                posInfo = dhAimPosInfo;
            }
            restPosInfo.localPos = posInfo.localPos;
            restPosInfo.rotation = Quaternion.Euler(posInfo.localRot);
            if (!_aiming)
            {
                _aiming = true;
                _sprinting = false;
                reset = true;
                _lerpToPos = posInfo.localPos;
                _lerpToRot = Quaternion.Euler(posInfo.localRot);
            }
        }
        //Sprinting bob camera resting position
        else if (PlayerIsSprinting && PlayerIsGrounded)
        {
            restPosInfo.localPos = sprintPosInfo.localPos;
            restPosInfo.rotation = _origRestPosInfo.rotation;
            if (!_sprinting)
            {
                _sprinting = true;
                _aiming = false;
                reset = true;
                _lerpToPos = sprintPosInfo.localPos;
                _lerpToRot = _origRestPosInfo.rotation;
            }
        }
        //Hip-aimed bob camera resting position
        else
        {
            restPosInfo.localPos = _origRestPosInfo.localPos;
            restPosInfo.rotation = _origRestPosInfo.rotation;
            if (_aiming || _sprinting)
            {
                _aiming = false;
                _sprinting = false;
                reset = true;
                _lerpToPos = _origRestPosInfo.localPos;
                _lerpToRot = _origRestPosInfo.rotation;
            }
        }
    }

    //Lerp to a new resting position reset out cycle
    private void HandleResetting()
    {
        //Lerp to the new resting position
        bobTarget.localPosition = Vector3.Lerp(bobTarget.localPosition, _lerpToPos, 20f * Time.deltaTime);
        bobTarget.localRotation = Quaternion.Lerp(bobTarget.localRotation, _lerpToRot, 20f * Time.deltaTime);
        //Snap to the new value once we get within a certain range
        if (LerpComplete(bobTarget.localPosition, _lerpToPos))
        {
            reset = false;
            bobTarget.localPosition = _lerpToPos;
            bobTarget.localRotation = _lerpToRot;
            //Reset our bob cycle
            _timer = Mathf.PI / 2;
        }
    }

    //Handle bobbing the camera or body left/right while moving
    private void HandleBob(bool doubleHanded)
    {
        //While we are moving apply a head/body bob
        if (PlayerIsGrounded 
            && (HorizontalRaw != 0 || VerticalRaw != 0))
        {
            //Determine bob speed + amount depending on player movement state
            float bobSpeed = walkBobSpeed;
            float bobAmount = walkBobAmount;
            if (PlayerIsSprinting)
            {
                bobSpeed = sprintBobSpeed;
                bobAmount = sprintBobAmount;
            }
            else if (PlayerIsCrouching)
            {
                bobSpeed = walkBobSpeed / 1.5f;
                bobAmount = walkBobAmount / 1.5f;
            }
            else if (PlayerIsAiming)
            {
                bobSpeed = walkBobSpeed / 1.5f;
                bobAmount = doubleHanded ? (walkBobAmount / 8f) : (walkBobAmount / 1.5f);
            }
            //Increase / decrease bobSpeed based on our controller input
            float inputSpeed = Mathf.Abs(HorizontalRaw) + Mathf.Abs(VerticalRaw);
            float inputSpeedClamped = Mathf.Clamp(inputSpeed, -1f, 1f);
            _timer += bobSpeed * Time.deltaTime * inputSpeedClamped;
            //Bounce the position left/right in a cycle according to the timer
            bobTarget.localPosition = new Vector3(restPosInfo.localPos.x + Mathf.Cos(_timer) * bobAmount, restPosInfo.localPos.y + Mathf.Abs((Mathf.Sin(_timer) * bobAmount)), restPosInfo.localPos.z);
        }
        else
        {
            //Reset the bob cycle
            _timer = Mathf.PI / 2;
            if (restPosInfo.localPos != bobTarget.localPosition && crouchPosInfo.localPos != bobTarget.localPosition)
            {
                Vector3 newPosition = new Vector3(
                    Mathf.Lerp(bobTarget.localPosition.x, restPosInfo.localPos.x, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(bobTarget.localPosition.y, restPosInfo.localPos.y, transitionSpeed * Time.deltaTime),
                    Mathf.Lerp(bobTarget.localPosition.z, restPosInfo.localPos.z, transitionSpeed * Time.deltaTime)
                );
                bobTarget.localPosition = newPosition;
            }
        }
    }

    //Handle tilting the body when moving horizontally
    private void HandleTilt(bool doubleHanded)
    {
        if (isCamera) return;
        //If we're moving Horizontally at all, apply a tilt to the hands
        if (HorizontalRaw != 0f)
        {
            Quaternion q = Quaternion.identity;
            if (inputState.playerIsAiming)
            {
                if (doubleHanded)
                {
                    Vector3 bueler = dhAimPosInfo.localRot;
                    float tiltAngle = HorizontalRaw * 1f;
                    q = Quaternion.Euler(bueler.x, bueler.y + tiltAngle, bueler.z);
                }
                else
                {
                    Vector3 bueler = shAimPosInfo.localRot;
                    float tiltAngle = HorizontalRaw * -6f;
                    q = Quaternion.Euler(bueler.x, bueler.y, bueler.z + tiltAngle);
                }
            }
            bobTarget.localRotation = Quaternion.Lerp(bobTarget.localRotation, q, 4f * Time.deltaTime);
        }
        //Otherwise keep us in the resting position
        else if (restPosInfo.rotation != bobTarget.localRotation)
        {
            float rotLerpSpeed = 4f * Time.deltaTime;
            if (inputState.playerIsAiming)
            {
                rotLerpSpeed = 20f * Time.deltaTime;
            }
            bobTarget.localRotation = Quaternion.Lerp(bobTarget.localRotation, restPosInfo.rotation, rotLerpSpeed);
        }
    }

    private bool LerpComplete(Vector3 startPos, Vector3 endPos)
    {
        if (Mathf.Abs(endPos.x - startPos.x) <= 0.001f
            && Mathf.Abs(endPos.y - startPos.y) <= 0.001f
            && Mathf.Abs(endPos.z - startPos.z) <= 0.001f)
        {
            return true;
        }
        return false;
    }

}