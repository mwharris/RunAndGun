using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WeaponPickup : AbstractBehavior
{
    public bool ShowPickupMessage = false;
    public string PickupWeaponName = null;

    private GameManager gm;
    private ItemDatabase itemDatabase;
    private PlayerInventory inventory;
    private BodyController bodyController;
    private Transform playerCamera;
    
    private float pickupRayLength = 5f;
    private bool pickedUpWeapon = false;
    private PickupRaycastInfo pickupRayInfo;

    [SerializeField] private Transform weaponPoint;
    [SerializeField] private WeaponSwitcher weaponSwitcher;
    
    //Inner class to help with function return
    class PickupRaycastInfo
    {
        public bool showPickupMessage = false;
        public string pickupWeaponName = "";
        public ItemInfo pickupWeaponInfo = null;
    }

    private void Start()
    {
        inventory = GetComponent<PlayerInventory>();
        gm = FindObjectOfType<GameManager>();
        itemDatabase = gm.GetComponent<ItemDatabase>();
        bodyController = GetComponent<BodyController>();
        playerCamera = bodyController.PlayerBodyData.playerCamera;
        pickupRayInfo = new PickupRaycastInfo();
    }

    void Update ()
    {
        //Get the amount of time we've held the Pick Up button
        float pickupBtn = inputState.GetButtonHoldTime(inputs[0]);

        //Raycast and determine if we're looking at a weapon we can pick up
        pickupRayInfo.showPickupMessage = false;
        pickupRayInfo.pickupWeaponName = null;
        pickupRayInfo.pickupWeaponInfo = null;
        DoPickupRaycast(pickupRayInfo);
        
        if (pickupRayInfo.showPickupMessage)
        {
            //Prompt the user to pickup the weapon
            ShowPickupMessage = true;
            PickupWeaponName = pickupRayInfo.pickupWeaponName;
            //Actually pickup the weapon if we are holding the button
            Debug.Log("Pickup Button Hold Time: " + pickupBtn);
            if (pickupBtn >= 0f && !pickedUpWeapon)
            {
                DoWeaponPickup(pickupRayInfo);
                pickedUpWeapon = true;
            }
        }
        else
        {
            ShowPickupMessage = false;
            PickupWeaponName = null;
        }

        //Reset our pickedUpWeapon flag whenever we release the held button
        if (pickupBtn <= 0f)
        {
            pickedUpWeapon = false;
        }

        //Debug.DrawRay(playerCamera.position, playerCamera.forward * pickupRayLength, Color.red);
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
                ItemInfo pickupItem = itemDatabase.GetItem(pickupInfo.itemId);
                //Make sure this item isn't already in our inventory
                if (!inventory.PlayerHasItem(pickupItem.itemId))
                {
                    pri.showPickupMessage = true;
                    pri.pickupWeaponName = pickupItem.itemName;
                    pri.pickupWeaponInfo = pickupItem;
                }
            }
        }
    }

    private void DoWeaponPickup(PickupRaycastInfo pri)
    {
        //Get a reference to our currently held weapon
        Transform currWeaponTransform = bodyController.PlayerBodyData.weapon;
        Item currWeaponItem = currWeaponTransform.GetComponent<Item>();

        GameObject newWeaponGo = null;
        if (weaponPoint != null)
        {
            //Instantiate our new weapon prefab
            newWeaponGo = Instantiate(pri.pickupWeaponInfo.handheldPrefab, weaponPoint, false);
            newWeaponGo.SetActive(false);

            //Update our PlayerBodyData with the new weapon transform
            bodyController.PlayerBodyData.weapon = newWeaponGo.transform;

            //Tell the inventory to add/replace this weapon
            Item newWeaponItem = newWeaponGo.GetComponent<Item>();
            int index = inventory.PlaceWeapon(newWeaponItem, currWeaponItem);

            //Make sure our new weapon gameObject is in the correct sibling location
            newWeaponGo.transform.SetSiblingIndex(index);

            //Switch to this new weapon
            weaponSwitcher.SwitchWeaponTo(index);
            
            //TODO: removed any replaced weapons
        }
        else
        {
            Debug.LogError("Weapon Point Not Set!!!");
        }
    }
}