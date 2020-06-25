using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : Photon.Pun.MonoBehaviourPun
{
    private PlayerBodyData playerBodyData;
    public PlayerBodyData PlayerBodyData
    {
        get { return playerBodyData;  }
        set { playerBodyData = value; }
    }

    [SerializeField] private PlayerBodyData firstPersonArms;
    public PlayerBodyData FirstPersonArms
    {
        get { return firstPersonArms; }
    }

    [SerializeField] private PlayerBodyData thirdPersonBody;
    public PlayerBodyData ThirdPersonBody
    {
        get { return thirdPersonBody; }
    }

    void Awake()
    {
        playerBodyData = firstPersonArms;
        if (!photonView.IsMine)
        {
            playerBodyData = thirdPersonBody;
        }
        else
        {
            playerBodyData = firstPersonArms;
        }
    }

    public void ForceThird()
    {
        playerBodyData = thirdPersonBody;
    }
}