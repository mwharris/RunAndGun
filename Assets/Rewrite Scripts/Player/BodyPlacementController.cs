using UnityEngine;

public class BodyPlacementController : AbstractBehavior
{
    [SerializeField] private Transform bodyTransform;
    
    private BodyController _bodyController;
    private PlayerMovementStateMachine _playerMovementStateMachine;

    private Vector3 _currWeaponDefaultPos;
    private Vector3 _currWeaponDefaultRot;
    private Vector3 _currWeaponCrouchPos;
    private Vector3 _currWeaponCrouchRot;
    private float _currWeaponLerpSpeed = 20f;
    private float _currWeaponLerpMultiplier = 0f;

    private float _defaultLerpSpeed;
    private float _defaultCrouchLerpSpeed;

    private bool PlayerIsCrouching => _playerMovementStateMachine.CurrentStateType == typeof(Crouching);
    private bool PlayerIsSliding => _playerMovementStateMachine.CurrentStateType == typeof(Sliding);
    
    void Start()
    {
        _bodyController = GetComponent<BodyController>();
        _playerMovementStateMachine = GetComponent<PlayerMovementStateMachine>();
    }
    
    void Update ()
    {
        SetBodyVars(inputState.playerIsAiming);
        HandleBodyPlacement(inputState.playerIsAiming);
    }

    //Set our various position and rotation vectors based on the current weapon
    private void SetBodyVars(bool isAiming)
    {
        _defaultLerpSpeed = Time.deltaTime * 20f;
        _defaultCrouchLerpSpeed = Time.deltaTime * 10f;
        if (_bodyController != null)
        {
            WeaponData currWeaponData = _bodyController.PlayerBodyData.GetWeaponData();
            _currWeaponDefaultPos = currWeaponData.DefaultArmsPosition.localPos;
            _currWeaponDefaultRot = currWeaponData.DefaultArmsPosition.localRot;
            _currWeaponCrouchPos = currWeaponData.CrouchArmsPosition.localPos;
            _currWeaponCrouchRot = currWeaponData.CrouchArmsPosition.localRot;
            _currWeaponLerpMultiplier = currWeaponData.KickReturnAimMultiplier;
            if (currWeaponData.KickReturnSpeed > 0)
            {
                _currWeaponLerpSpeed = currWeaponData.KickReturnSpeed;
            }
            else if (PlayerIsCrouching && !isAiming)
            {
                _currWeaponLerpSpeed = _defaultCrouchLerpSpeed;
            }
            else
            {
                _currWeaponLerpSpeed = _defaultLerpSpeed;
            }
        }
    }

    void HandleBodyPlacement(bool isAiming)
    {
        //Lerp variables
        Vector3 currPos = bodyTransform.transform.localPosition;
        Quaternion currRot = bodyTransform.transform.localRotation;

        if (PlayerIsCrouching || PlayerIsSliding)
        {
            //If both Aiming and Crouching, return to normal position.
            //BobController will handle the aiming.
            if (isAiming)
            {
                if (_currWeaponLerpMultiplier > 0)
                {
                    _currWeaponLerpSpeed *= _currWeaponLerpMultiplier;
                }
                currPos = Vector3.Lerp(currPos, _currWeaponDefaultPos, _currWeaponLerpSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(_currWeaponDefaultRot), _currWeaponLerpSpeed);
            }
            else
            {
                currPos = Vector3.Lerp(currPos, _currWeaponCrouchPos, _currWeaponLerpSpeed);
                currRot = Quaternion.Lerp(currRot, Quaternion.Euler(_currWeaponCrouchRot), _currWeaponLerpSpeed);
            }
        }
        else
        {
            currPos = Vector3.Lerp(currPos, _currWeaponDefaultPos, _currWeaponLerpSpeed);
            currRot = Quaternion.Lerp(currRot, Quaternion.Euler(_currWeaponDefaultRot), _currWeaponLerpSpeed);
        }

        bodyTransform.transform.localPosition = currPos;
        bodyTransform.transform.localRotation = currRot;
    }
}
