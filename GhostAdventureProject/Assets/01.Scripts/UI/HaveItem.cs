using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaveItem : MonoBehaviour
{
    [Header("에디터에서 아이템 추가")]
    public List<ItemData> initialItems = new List<ItemData>();

    [HideInInspector]
    public List<InventorySlot_PossessableObject> inventorySlots = new List<InventorySlot_PossessableObject>();

    private void Awake()
    {
        ConvertToSlots();
    }

    private void ConvertToSlots()
    {
        inventorySlots.Clear();

        foreach (var item in initialItems)
        {
            var existingSlot = inventorySlots.Find(slot => slot.item == item);
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
