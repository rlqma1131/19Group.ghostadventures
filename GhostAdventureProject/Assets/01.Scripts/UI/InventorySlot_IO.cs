using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventorySlot_IO : MonoBehaviour
{
    public ItemData item;
    public int count;

    // UI 관련 참조
    private GameObject slotUI;
    private Image iconImage;
    private TextMeshProUGUI countText;

    public InventorySlot_IO(ItemData item, int count, GameObject slotPrefab, Transform parent)
    {
        this.item = item;
        this.count = count;

        // 슬롯 UI 생성 및 설정
        slotUI = GameObject.Instantiate(slotPrefab, parent);
        iconImage = slotUI.transform.Find("Icon").GetComponent<Image>();
        countText = slotUI.transform.Find("CountText").GetComponent<TextMeshProUGUI>();

        iconImage.sprite = item.Item_Icon;
        UpdateUI();
    }

    public void AddItem(int amount = 1)
    {
        count += amount;
        UpdateUI();
    }

    public void UseItem(int amount = 1)
    {
        count -= amount;
        if (count < 0) count = 0;
        UpdateUI();
    }

    public bool IsEmpty()
    {
        return count <= 0;
    }

    public void DestroySlotUI()
    {
        if (slotUI != null)
        {
            GameObject.Destroy(slotUI);
        }
    }

    public void UpdateUI()
    {
        if (countText != null)
            countText.text = count.ToString();
    }




//     public Image icon;
//     public TextMeshProUGUI clueName;

//     public void Setup(ItemData item)
//     {
//         icon.sprite = item.Item_Icon;
//         icon.enabled = true; // 아이콘 표시
//         clueName.text = clue.clue_Name;
//     }

//     internal void Clear()
//     {
//         icon.sprite = null;
//         icon.enabled = false; // 아이콘 숨기기
//     }
}
