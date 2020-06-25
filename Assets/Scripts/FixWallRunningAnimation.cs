using Photon.Pun;
using UnityEngine;

public class FixWallRunningAnimation : AbstractBehavior
{
    public Vector3 wallRunLeftTargetRot = Vector3.zero;
    public Vector3 wallRunRightTargetRot = Vector3.zero;
    private Quaternion _origBodyRotation;
    private Quaternion _currBodyRotation;

    public Vector3 leftHandWallRunWeaponPosition;
    public Vector3 leftHandWallRunWeaponRotation;
    public Vector3 rightHandWallRunWeaponPosition;
    public Vector3 rightHandWallRunWeaponRotation;
    private Vector3 _origWeaponPosition;
    private Quaternion _origWeaponRotation;

    private PhotonView _pView;
    private BodyController _bodyController;
    private PlayerBodyData _playerBodyData;

    private void Start()
    {
        _pView = GetComponent<PhotonView>();
        _bodyController = GetComponent<BodyController>();
        _playerBodyData = _bodyController.PlayerBodyData;
        //Store original body and weapon position / rotations for later calculations
        _origBodyRotation = _playerBodyData.body.localRotation;
        _currBodyRotation = _origBodyRotation;
        _origWeaponPosition = _playerBodyData.weapon.localPosition;
        _origWeaponRotation = _playerBodyData.weapon.localRotation;
    }

    public void RunFix(bool wrLeft, bool wrRight, float deltaTime)
    {
        if (!_pView.IsMine)
        {
            HandleBodyRotation(wrLeft, wrRight, deltaTime);
            HandleWeaponPlacement(wrLeft, wrRight);
        }
    }

    //Rotate the player's body 90-degrees depending on wall-run side.
    //This is to overcome an issue with the animations being rotated 90-degrees.
    void HandleBodyRotation(bool wrLeft, bool wrRight, float deltaTime)
    {
        if (wrLeft || wrRight || _origBodyRotation != _currBodyRotation)
        {
            if (wrLeft)
            {
                Quaternion rot = Quaternion.Euler(wallRunLeftTargetRot.x, wallRunLeftTargetRot.y, wallRunLeftTargetRot.z);
                _currBodyRotation = Quaternion.Lerp(_currBodyRotation, rot, deltaTime * 5f);
            }
            else if (wrRight)
            {
                Quaternion rot = Quaternion.Euler(wallRunRightTargetRot.x, wallRunRightTargetRot.y, wallRunRightTargetRot.z);
                _currBodyRotation = Quaternion.Lerp(_currBodyRotation, rot, deltaTime * 5f);
            }
            else
            {
                _currBodyRotation = Quaternion.Lerp(_currBodyRotation, _origBodyRotation, deltaTime * 10f);
            }
            _playerBodyData.body.localRotation = _currBodyRotation;
        }
    }

    //When wall-running, one hand is on the wall and one hand aims/holds the weapon.
    //This will handle swapping the weapon to the opposite hand when wall-running right.
    void HandleWeaponPlacement(bool wrLeft, bool wrRight)
    {
        if (wrLeft)
        {
            _playerBodyData.weapon.SetParent(_playerBodyData.rightHandTarget);
            _playerBodyData.weapon.localPosition = rightHandWallRunWeaponPosition;
            _playerBodyData.weapon.localRotation = Quaternion.Euler(rightHandWallRunWeaponRotation);
        }
        else if (wrRight)
        {
            _playerBodyData.weapon.SetParent(_playerBodyData.leftHandTarget);
            _playerBodyData.weapon.localPosition = leftHandWallRunWeaponPosition;
            _playerBodyData.weapon.localRotation = Quaternion.Euler(leftHandWallRunWeaponRotation);
        }
        else
        {
            _playerBodyData.weapon.SetParent(_playerBodyData.rightHandTarget);
            _playerBodyData.weapon.localPosition = _origWeaponPosition;
            _playerBodyData.weapon.localRotation = _origWeaponRotation;
        }
    }

}
