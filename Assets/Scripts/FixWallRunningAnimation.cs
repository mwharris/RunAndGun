using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FixWallRunningAnimation : AbstractBehavior
{
    public Vector3 wallRunLeftTargetRot = Vector3.zero;
    public Vector3 wallRunRightTargetRot = Vector3.zero;
    private Quaternion origBodyRotation;
    private Quaternion currBodyRotation;

    public Vector3 leftHandWallRunWeaponPosition;
    public Vector3 leftHandWallRunWeaponRotation;
    public Vector3 rightHandWallRunWeaponPosition;
    public Vector3 rightHandWallRunWeaponRotation;
    private Vector3 origWeaponPosition;
    private Quaternion origWeaponRotation;

    //private Vector3 origLocalPos;
    //private Quaternion origLocalRot;

    private PhotonView pView;
    private BodyController bodyController;
    private PlayerBodyData playerBodyData;

    private void Start()
    {
        pView = GetComponent<PhotonView>();
        bodyController = GetComponent<BodyController>();
        playerBodyData = bodyController.PlayerBodyData;
        //Store original body and weapon position / rotations for later calculations
        origBodyRotation = playerBodyData.body.localRotation;
        currBodyRotation = origBodyRotation;
        origWeaponPosition = playerBodyData.weapon.localPosition;
        origWeaponRotation = playerBodyData.weapon.localRotation;
        //origLocalPos = playerBodyData.body.localPosition;
        //origLocalRot = playerBodyData.body.localRotation;
    }

    public void RunFix(bool wrLeft, bool wrRight, float deltaTime)
    {
        if (!pView.isMine)
        {
            HandleBodyRotation(wrLeft, wrRight, deltaTime);
            HandleWeaponPlacement(wrLeft, wrRight);
        }
    }

    //Rotate the player's body 90-degrees depending on wall-run side.
    //This is to overcome an issue with the animations being rotated 90-degrees.
    void HandleBodyRotation(bool wrLeft, bool wrRight, float deltaTime)
    {
        if (wrLeft || wrRight || origBodyRotation != currBodyRotation)
        {
            if (wrLeft)
            {
                Quaternion rot = Quaternion.Euler(wallRunLeftTargetRot.x, wallRunLeftTargetRot.y, wallRunLeftTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, deltaTime * 5f);
            }
            else if (wrRight)
            {
                Quaternion rot = Quaternion.Euler(wallRunRightTargetRot.x, wallRunRightTargetRot.y, wallRunRightTargetRot.z);
                currBodyRotation = Quaternion.Lerp(currBodyRotation, rot, deltaTime * 5f);
            }
            else
            {
                currBodyRotation = Quaternion.Lerp(currBodyRotation, origBodyRotation, deltaTime * 10f);
            }
            playerBodyData.body.localRotation = currBodyRotation;
        }
    }

    //When wall-running, one hand is on the wall and one hand aims/holds the weapon.
    //This will handle swapping the weapon to the opposite hand when wall-running right.
    void HandleWeaponPlacement(bool wrLeft, bool wrRight)
    {
        if (wrLeft)
        {
            playerBodyData.weapon.SetParent(playerBodyData.rightHandTarget);
            playerBodyData.weapon.localPosition = rightHandWallRunWeaponPosition;
            playerBodyData.weapon.localRotation = Quaternion.Euler(rightHandWallRunWeaponRotation);
        }
        else if (wrRight)
        {
            playerBodyData.weapon.SetParent(playerBodyData.leftHandTarget);
            playerBodyData.weapon.localPosition = leftHandWallRunWeaponPosition;
            playerBodyData.weapon.localRotation = Quaternion.Euler(leftHandWallRunWeaponRotation);
        }
        else
        {
            playerBodyData.weapon.SetParent(playerBodyData.rightHandTarget);
            playerBodyData.weapon.localPosition = origWeaponPosition;
            playerBodyData.weapon.localRotation = origWeaponRotation;
        }
    }

}
