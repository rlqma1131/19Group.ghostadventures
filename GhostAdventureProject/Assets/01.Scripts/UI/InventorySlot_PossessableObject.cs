using TMPro;
using UnityEngine;
using UnityEngine.UI;

    [System.Serializable]

public class InventorySlot_PossessableObject : MonoBehaviour
{
    public ItemData item;
    public Image iconImage;
    public int quantity;
    private GameObject slotUI;
    private TextMeshProUGUI countText;

    public TextMeshProUGUI quantityText;
    private Inventory_PossessableObject inventoryRef;


    // public InventorySlot_PossessableObject(ItemData item, GameObject slotPrefab, Transform parent, int quantity = 1)
    // {
    //     this.item = item;
    //     this.quantity = quantity;

    //     // UI 생성
    //     slotUI = GameObject.Instantiate(slotPrefab, parent);
    //     iconImage = slotUI.transform.Find("Icon").GetComponent<Image>();
    //     countText = slotUI.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

    //     iconImage.sprite = item.Item_Icon;
    //     UpdateUI();
    // }
    public InventorySlot_PossessableObject(ItemData item, int quantity = 1)
    {
        this.item = item;
        this.quantity = quantity;
    }

    public void SetSlot(InventorySlot_PossessableObject slot)
    {
        iconImage.sprite = slot.item.Item_Icon;
        quantityText.text = slot.quantity > 1 ? slot.quantity.ToString() : "";
        inventoryRef = Inventory_PossessableObject.Instance;
        item = slot.item;
    }  

    // public void Setup(ItemData itemData, Inventory_PossessableObject inventory)
    // {
    //     item = itemData;
    //     inventoryRef = inventory;
    //     // UI 표시 업데이트 등
    // }

    public void OnClick()
    {
        inventoryRef.UseOrPlaceItem(item);
    }


    /// ✅ 아이템 수량 증가
    public void AddItem(int amount = 1)
    {
        quantity += amount;
        UpdateUI();
    }

    /// ✅ 아이템 수량 감소
    public void UseItem(int amount)
    {
        quantity -= amount;
        if (quantity < 0) quantity = 0;
        UpdateUI();
    }

    /// ✅ 수량 0인지 확인
    public bool IsEmpty()
    {
        return quantity <= 0;
    }

    /// ✅ 슬롯 UI 제거
    public void DestroySlotUI()
    {
        // if (slotUI != null)
        // {
        //     GameObject.Destroy(slotUI);
        // }
        item = null;
        iconImage = null;
    }

    /// ✅ UI 텍스트 갱신
    public void UpdateUI()
    {
        if (countText != null)
        {
            countText.text = quantity.ToString();
        }
    }
}

