using TMPro;
using UnityEngine;
using UnityEngine.UI;

    [System.Serializable]

public class InventorySlot_PossessableObject : MonoBehaviour
{
    public ItemData item;
    public Image iconImage;
    public int quantity;
    public int count;
    private GameObject slotUI;
    private TextMeshProUGUI countText;

    public TextMeshProUGUI quantityText;
    private Inventory_PossessableObject inventoryRef;


    public InventorySlot_PossessableObject(ItemData item, int count, GameObject slotPrefab, Transform parent, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
        this.count = count;

        // UI 생성
        slotUI = GameObject.Instantiate(slotPrefab, parent);
        iconImage = slotUI.transform.Find("Icon").GetComponent<Image>();
        countText = slotUI.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

        iconImage.sprite = item.Item_Icon;
        UpdateUI();
    }
    public InventorySlot_PossessableObject(ItemData item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
    }
    public void SetSlot(InventorySlot_PossessableObject slot)
    {
        iconImage.sprite = slot.item.Item_Icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
    }  

    public void Setup(ItemData itemData, int count, Inventory_PossessableObject inventory)
    {
        item = itemData;
        inventoryRef = inventory;
        // UI 표시 업데이트 등
    }

    public void OnClick()
    {
        inventoryRef.UseOrPlaceItem(item);
    }


    /// ✅ 아이템 수량 증가
    public void AddItem(int amount = 1)
    {
        count += amount;
        UpdateUI();
    }

    /// ✅ 아이템 수량 감소
    public void UseItem(int amount = 1)
    {
        count -= amount;
        if (count < 0) count = 0;
        UpdateUI();
    }

    /// ✅ 수량 0인지 확인
    public bool IsEmpty()
    {
        return count <= 0;
    }

    /// ✅ 슬롯 UI 제거
    public void DestroySlotUI()
    {
        if (slotUI != null)
        {
            GameObject.Destroy(slotUI);
        }
    }

    /// ✅ UI 텍스트 갱신
    public void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = count.ToString();
        }
    }
}

