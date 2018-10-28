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
        //Find the GameObject of the player who shot
        GameObject shooter = playerFinder.FindOtherPlayerByPUNId(punId);
        if (shooter != null)
        {
            //Mark their animator has having shot
            BodyController bodyControl = shooter.GetComponent<BodyController>();
            bodyControl.PlayerBodyData.bodyAnimator.SetTrigger("ShootTrig");
        }
    }

    [PunRPC]
    void PlayerReloaded(int punId)
    {
        //Find the GameObject of the player who reloaded
        GameObject reloader = playerFinder.FindOtherPlayerByPUNId(punId);
        if (reloader != null)
        {
            //Mark their animator has having shot
            BodyController bodyControl = reloader.GetComponent<BodyController>();
            bodyControl.PlayerBodyData.bodyAnimator.SetTrigger("ReloadTrig");
        }
    }

}
