using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ReloadIndicator : MonoBehaviour
{
    private GameManager gameManager;
    private GameObject player;
    private ShootController shootController;
    private GameObject reloadTxt;

    void Start()
    {
        gameManager = FindObjectOfType<GameManager>();
        reloadTxt = transform.GetChild(0).gameObject;
    }

    void Update()
    {
        if (player == null)
        {
            player = gameManager.MyPlayer;
        }
        if (player != null && shootController == null)
        {
            shootController = player.GetComponent<ShootController>();
        }
        if (player != null && shootController != null && shootController.BulletCount <= 2)
        {
            reloadTxt.SetActive(true);
        }
        else
        {
            reloadTxt.SetActive(false);
        }
    }
}
