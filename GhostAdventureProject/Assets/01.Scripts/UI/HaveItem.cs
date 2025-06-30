using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HaveItem : MonoBehaviour
{
    [Header("초기 아이템 설정")]
    public List<ItemData> defaultItems = new List<ItemData>();
    public List<int> defaultCounts = new List<int>();

    [HideInInspector]
    public List<InventorySlot_IO> haveItemSlots = new List<InventorySlot_IO>();

    private void Awake()
    {
        InitializeSlots();
    }

    private void InitializeSlots()
    {
        haveItemSlots.Clear();

        for (int i = 0; i < defaultItems.Count; i++)
        {
            ItemData item = defaultItems[i];
            // int count = (i < defaultCounts.Count) ? defaultCounts[i] : 1;
            // haveItemSlots.Add(new InventorySlot_IO(item, count, null, null));
        }
    }
}
