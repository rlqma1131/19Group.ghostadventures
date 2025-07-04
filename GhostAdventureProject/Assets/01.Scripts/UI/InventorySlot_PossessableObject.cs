using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventorySlot_PossessableObject : MonoBehaviour
{
    public ItemData item;
    public int quantity;
    public Image iconImage;
    public TextMeshProUGUI quantityText;


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

}

