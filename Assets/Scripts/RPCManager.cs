using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPCManager : MonoBehaviour {

    private PlayerFinder playerFinder;

    private void Awake()
    {
        playerFinder = new PlayerFinder();
    }

    [PunRPC]
    void PlayerShot(int punId)
    {
        //Mark their animator has having shot
        Animator playerAnim = GetPlayerBodyAnimatorByPunID(punId);
        if (playerAnim != null)
        {
            playerAnim.SetTrigger("ShootTrig");
        }
    }

    [PunRPC]
    void PlayerReloaded(int punId)
    {
        //Mark their animator has having shot
        Animator playerAnim = GetPlayerBodyAnimatorByPunID(punId);
        if (playerAnim != null)
        {
            playerAnim.SetTrigger("ReloadTrig");
        }
    }


    [PunRPC]
    void PlayerWeaponChange(int punId, int itemId)
    {
        //Get the player we are controlling
        GameObject player = GetMyPlayer(punId);
        //Grab a reference to ThirdPersonWeaponSwitcher via BodyController
        BodyController bc = player?.GetComponent<BodyController>();
        Transform rightHand = bc?.PlayerBodyData.rightHandTarget;
        ThirdPersonWeaponSwitcher weaponSwitcher = rightHand?.GetComponent<ThirdPersonWeaponSwitcher>();
        //Tell the ThirdPersonWeaponSwitcher to change our weapon
        weaponSwitcher.SwapWeaponTo(itemId, bc.PlayerBodyData.body);
    }

    private GameObject GetMyPlayer(int punId)
    {
        return playerFinder.FindOtherPlayerByPUNId(punId);
    }

    private Animator GetPlayerBodyAnimatorByPunID(int punId)
    {
        GameObject player = GetMyPlayer(punId);
        if (player != null)
        {
            BodyController bodyControl = player.GetComponent<BodyController>();
            return bodyControl.PlayerBodyData.GetBodyAnimator();
        }
        return null;
    }

}
