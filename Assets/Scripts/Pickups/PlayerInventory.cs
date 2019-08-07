using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    [SerializeField] private Item weapon1;
    [SerializeField] private Item weapon2;

    public bool PlayerHasItem(int id)
    {
        if (weapon1 != null && weapon1.info.itemId == id)
        {
            return true;
        }
        if (weapon2 != null && weapon2.info.itemId == id)
        {
            return true;
        }
        return false;
    }

    public Item GetItemById(int id)
    {
        if (weapon1 != null && weapon1.info.itemId == id)
        {
            return weapon1;
        }
        if (weapon2 != null && weapon2.info.itemId == id)
        {
            return weapon2;
        }
        return null;
    }

    public bool IsInventoryFull()
    {
        return weapon1 != null && weapon2 != null;
    }

    /*
     * Place a new weapon into our inventory.
     * Handles replacing currently held weapon AND filling empty slots.
     * Returns the index of the place weapon.
     */
    public int PlaceWeapon(Item newWeaponItem, Item currWeaponItem)
    {
        int index = 0;
        //Replace our currently held weapon
        if (IsInventoryFull())
        {
            if (weapon1 == currWeaponItem)
            {
                weapon1 = newWeaponItem;
            }
            else if (weapon2 == currWeaponItem)
            {
                weapon2 = newWeaponItem;
                index = 1;
            }
            else
            {
                Debug.LogError("PlaceWeapon() failed to place the weapon!");
            }
        }
        //Add the new weapon to an open slot
        else
        {
            if (weapon1 == null)
            {
                weapon1 = newWeaponItem;
            }
            else if (weapon2 == null)
            {
                weapon2 = newWeaponItem;
                index = 1;
            }
            else
            {
                Debug.LogError("PlaceWeapon() failed to place the weapon!");
            }
        }
        return index;
    }
}
