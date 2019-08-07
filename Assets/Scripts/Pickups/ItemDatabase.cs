using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemDatabase : MonoBehaviour
{
    [SerializeField] public ItemInfo[] items;

    //Get an item from the database by ID, IDs match the index
    public ItemInfo GetItem(int id)
    {
        return items[id];
    }
}
