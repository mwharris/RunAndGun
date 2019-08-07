using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class PickupIndicator : MonoBehaviour
{
    private GameObject player;
    private GameManager gameManager;
    private WeaponPickup pickupScript;
    private GameObject pickupTxtGo;
    private TextMeshProUGUI tmpText;

    private const string pickupMessage = "Hold F / X to pickup";

    void Start ()
    {
        gameManager = FindObjectOfType<GameManager>();
        pickupTxtGo = transform.GetChild(0).gameObject;
        tmpText = pickupTxtGo.GetComponent<TextMeshProUGUI>();
    }
	
	void Update ()
    {
        if (player == null)
        {
            player = gameManager.MyPlayer;
        }
        if (player != null && pickupScript == null)
        {
            pickupScript = player.GetComponent<WeaponPickup>();
        }
        if (player != null && pickupScript != null && pickupScript.ShowPickupMessage)
        {
            pickupTxtGo.SetActive(true);
            tmpText.text = pickupMessage + " " + pickupScript.PickupWeaponName;
        }
        else
        {
            pickupTxtGo.SetActive(false);
        }
    }
}
