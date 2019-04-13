using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BodyController : Photon.MonoBehaviour
{
    private PlayerBodyData playerBodyData;
    public PlayerBodyData PlayerBodyData
    {
        get { return playerBodyData;  }
        set { playerBodyData = value; }
    }

    [SerializeField] private PlayerBodyData firstPersonArms;
    [SerializeField] private PlayerBodyData thirdPersonBody;

    void Awake()
    {
        if (!photonView.isMine)
        {
            playerBodyData = thirdPersonBody;
            //playerBodyData = firstPersonArms;
        }
        else
        {
            playerBodyData = firstPersonArms;
            //playerBodyData = thirdPersonBody;
        }
    }

    public void ForceThird()
    {
        playerBodyData = thirdPersonBody;
    }
}
