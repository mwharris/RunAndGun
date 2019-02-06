using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerFinder {

    //Iterate through players and find the one with the given ID (include our own)
    public GameObject FindPlayerByPUNId(int pId)
    {
        //Get all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //Loop through and find the player we are controlling
        foreach (GameObject currPlayer in players)
        {
            PhotonView pView = currPlayer.GetComponent<PhotonView>();
            if (pView.owner != null && pView.owner.ID == pId)
            {
                return currPlayer;
            }
        }
        return null;
    }

    //Iterate through players and find the one with the given ID (exclude our own)
    public GameObject FindOtherPlayerByPUNId(int pId)
    {
        //Get all players
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        //Loop through and find the player that isn't us with the given ID
        foreach (GameObject currPlayer in players)
        {
            PhotonView pView = currPlayer.GetComponent<PhotonView>();
            if (!pView.isMine && pView.owner != null && pView.owner.ID == pId)
            {
                return currPlayer;
            }
        }
        return null;
    }

}
