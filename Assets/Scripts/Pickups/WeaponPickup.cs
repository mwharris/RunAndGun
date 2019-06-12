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

    public float pickupRayLength = 1f;

    public float layerMaskNum = 6;

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
        bool showPickupMessage = false;
        string pickupName = "";
        int layerMask = 1 << 9;  //We only want to raycast on the Pickups layer (layer 9)

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
                    showPickupMessage = true;
                    pickupName = pickupItem.itemName;
                }
            }
        }

        //Control the pickup message through the MenuController
        if (showPickupMessage)
        {
            menuController.SetPickupAvailable(true, pickupName);
        }
        else
        {
            menuController.SetPickupAvailable(false, null);
        }

        Debug.DrawRay(playerCamera.position, playerCamera.forward * pickupRayLength, Color.red);
    }
}