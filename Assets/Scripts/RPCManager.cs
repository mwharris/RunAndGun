using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RPCManager : MonoBehaviour {

    [PunRPC]
    void PlayerShot(string punName)
    {
        //Find the GameObject of the player who shot
        GameObject shooter = FindPlayerByPUNName(punName);
        if (shooter != null)
        {
            //Mark their animator has having shot
            BodyController bodyControl = shooter.GetComponent<BodyController>();
            bodyControl.PlayerBodyData.bodyAnimator.SetTrigger("ShootTrig");
        }
    }

    [PunRPC]
    void PlayerReloaded(string punName)
    {
        //Find the GameObject of the player who reloaded
        GameObject reloader = FindPlayerByPUNName(punName);
        if (reloader != null)
        {
            //Mark their animator has having shot
            BodyController bodyControl = reloader.GetComponent<BodyController>();
            bodyControl.PlayerBodyData.bodyAnimator.SetTrigger("ReloadTrig");
        }
    }

    //Iterate through the players and find ours
    private GameObject FindPlayerByPUNName(string pName)
    {
        //Get all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //Loop through and find the player we are controlling
        foreach (GameObject currPlayer in players)
        {
            PhotonView pView = currPlayer.GetComponent<PhotonView>();
            if (!pView.isMine && pView.owner != null && pView.owner.NickName == pName)
            {
                return currPlayer;
            }
        }
        return null;
    }

}
