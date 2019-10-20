using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadIndicator : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject player;
    private GameObject reloadTxt;
    private BodyController bodyController;
    private PlayerBodyData playerBodyData;
    private WeaponData currWeaponData;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        reloadTxt = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        SetVars();
        if (player != null && currWeaponData != null && currWeaponData.BulletCount <= 2)
        {
            reloadTxt.SetActive(true);
        }
        else
        {
            reloadTxt.SetActive(false);
        }
    }

    private void SetVars()
    {
        if (player == null)
        {
            player = gameManager.MyPlayer;
        }
        if (player != null)
        {
            if (bodyController == null) {
                bodyController = player.GetComponent<BodyController>();
            }
            playerBodyData = bodyController.PlayerBodyData;
            currWeaponData = playerBodyData.weapon.GetComponent<WeaponData>();
        }
    }
}