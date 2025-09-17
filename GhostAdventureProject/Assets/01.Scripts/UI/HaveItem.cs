using System.Collections.Generic;
using System.Linq;
using UnityEngine;

// 빙의 오브젝트가 갖고 있는 아이템을 빙의인벤토리에 표시해주는 스크립트입니다.
// 빙의 오브젝트에 컴포넌트로 추가하고 initialItems에 ItemData(SO)를 넣으세요.

public class HaveItem : MonoBehaviour
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

    // 빙의인벤토리가 비었는지 확인
    public bool IsInventoryEmpty()
    {
        return inventorySlots.All(slot => slot.item == null ||  slot.quantity <= 0);
    }

    // 빙의인벤토리에 targetItemName 을 가진 item을 갖고 있는지 확인
    public bool IsHasItem(string targetItemName)
    {
        return inventorySlots.Any(slot => slot.item.Item_Name == targetItemName);
    }
}
