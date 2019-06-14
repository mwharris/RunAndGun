using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : AbstractBehavior
{
    private GameManager gm;
    private ItemDatabase itemDatabase;
    private PlayerInventory inventory;
    private BodyController bodyController;
    private MenuController menuController;
    private Transform playerCamera;

    private float pickupRayLength = 5f;
    private bool pickedUpWeapon = false;

    //Inner class to help with function return
    class PickupRaycastInfo
    {
        public bool showPickupMessage = false;
        public string pickupWeaponName = "";
        public ItemInfo pickupWeaponInfo;
    }

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        gm = FindObjectOfType<GameManager>();
        menuController = FindObjectOfType<MenuController>();
        itemDatabase = gm.GetComponent<ItemDatabase>();
        bodyController = GetComponent<BodyController>();
        playerCamera = bodyController.PlayerBodyData.playerCamera;
    }

    void Update ()
    {
        //Get the amount of time we've held the Pick Up button
        float pickupBtn = inputState.GetButtonHoldTime(inputs[0]);

        //Raycast and determine if we're looking at a weapon we can pick up
        PickupRaycastInfo pri = new PickupRaycastInfo();
        DoPickupRaycast(pri);

        //If we held the button for a little while looking at a weapon, pick up the weapon
        if (pri.showPickupMessage)
        {
            if (pickupBtn >= 1f && !pickedUpWeapon)
            {
                pickedUpWeapon = true;
                Debug.ClearDeveloperConsole();
                Debug.Log("PICKUP WEAPON!");
            }
        }

        //Reset our pickedUpWeapon flag whenever we release the held button
        if (pickupBtn <= 0f)
        {
            pickedUpWeapon = false;
            Debug.ClearDeveloperConsole();
            Debug.Log("CAN PICKUP AGAIN!");
        }

        Debug.DrawRay(playerCamera.position, playerCamera.forward * pickupRayLength, Color.red);
    }

    private void DoPickupRaycast(PickupRaycastInfo pri)
    {
        //We only want to raycast on the Pickups layer (layer 9)
        int layerMask = 1 << 9;  

        //Raycast out from our look position
        RaycastHit hit = new RaycastHit();
        Ray pickupRay = new Ray(playerCamera.position, playerCamera.forward);
        if (Physics.Raycast(pickupRay, out hit, pickupRayLength, layerMask))
        {
            GameObject hitGo = hit.collider.gameObject;
            Transform hitTrans = hit.collider.transform;
            //Grab the Item ID of the item we are looking at
            ItemId pickupInfo = hitGo.GetComponent<ItemId>();
            if (pickupInfo != null)
            {
                //Look this item up in the database
                ItemInfo pickupItem = itemDatabase.getItem(pickupInfo.itemId);
                //Check if this item is already in our inventory
                if (!inventory.PlayerHasItem(pickupItem.itemId))
                {
                    pri.showPickupMessage = true;
                    pri.pickupWeaponName = pickupItem.itemName;
                }
            }
        }

        //Control the pickup message through the MenuController
        if (pri.showPickupMessage)
        {
            menuController.SetPickupAvailable(true, pri.pickupWeaponName);
        }
        else
        {
            menuController.SetPickupAvailable(false, null);
        }
    }
}