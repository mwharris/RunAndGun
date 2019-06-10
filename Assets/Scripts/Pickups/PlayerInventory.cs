using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInventory : MonoBehaviour
{
    public Item weapon1;
    public Item weapon2;

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
}
