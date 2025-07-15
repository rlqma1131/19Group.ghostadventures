using System.Collections;
using System.Collections.Generic;
//using UnityEditor.Search;
using UnityEngine;

public class HaveItem : Singleton<HaveItem>
{
    [Header("ItemData Scriptable Object를 넣어주세요")]
    public List<ItemData> initialItems = new List<ItemData>();

    [HideInInspector]
    public List<InventorySlot_PossessableObject> inventorySlots = new List<InventorySlot_PossessableObject>();

    private void Start()
    {
        ConvertToSlots();
    }

    private void ConvertToSlots()
    {
        inventorySlots.Clear();

        foreach (var item in initialItems)
        {
            var existingSlot = inventorySlots.Find(slot => slot.item.Item_Name == item.Item_Name);
            if (existingSlot != null)
            {
                existingSlot.quantity++;
            }
            else
            {
                inventorySlots.Add(new InventorySlot_PossessableObject(item, 1));
            }
        }
    }
}
