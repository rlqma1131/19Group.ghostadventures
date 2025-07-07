using System.Collections.Generic;
using UnityEngine;

public class Inventory_PossessableObject : MonoBehaviour
{
    public Transform slotParent;
    public GameObject slotPrefab;

    private ItemData itemToPlace;
    private bool isPlacing = false;
    private GameObject previewInstance;

    private List<GameObject> spawnedSlots = new List<GameObject>();


    void Start()
    {
        UIManager.Instance.Inventory_PossessableObjectUI.gameObject.SetActive(false);
    }

    // 미사용(보류)
    BasePossessable FindPossessed()
    {
        BasePossessable[] all = FindObjectsOfType<BasePossessable>();
        foreach (var obj in all)
        {
            if (obj.IsPossessed)
                Debug.Log("obj:" + obj);
                return obj;
        }

        return null;
    }

    public void ShowInventory(List<InventorySlot_PossessableObject> slots)
    {
        Clear();

        foreach (var slot in slots)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            obj.GetComponent<InventorySlot_PossessableObject>().SetSlot(slot);
            spawnedSlots.Add(obj);
        }

        gameObject.SetActive(true);
    }

    public void HideInventory()
    {
        Clear();
        gameObject.SetActive(false);
    }

    private void Clear()
    {
        foreach (var slot in spawnedSlots)
        {
            Destroy(slot);
        }
        spawnedSlots.Clear();
    }

    public void OpenInventory(BasePossessable target)
    {
        HaveItem haveItem = target.GetComponent<HaveItem>();
        if (haveItem != null)
        {
            ShowInventory(haveItem.inventorySlots);
            return;
        }
        HideInventory();

    }

    public void CloseInventory()
    {
        HideInventory();
    }

    public void UseOrPlaceItem(ItemData item)
    {
        if (item.Item_Type == ItemType.Consumable || item.Item_Type == ItemType.Both)
        {
            // 아이템 사용 로직
            Debug.Log($"사용: {item.Item_Name}");
            UseItem(item, 1);
        }

        if (item.Item_Type == ItemType.Placeable || item.Item_Type == ItemType.Both)
        {
            // 설치 모드 진입
            Debug.Log($"설치 시작: {item.Item_Name}");
            StartPlacingItem(item);
        }
    }

    public void StartPlacingItem(ItemData item)
    {
        itemToPlace = item;
        isPlacing = true;

        if (previewInstance != null)
            Destroy(previewInstance);

        // 미리보기 프리팹 생성
        previewInstance = Instantiate(item.placeablePrefab);
        var sr = previewInstance.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = new Color(1f, 1f, 1f, 0.5f); // 반투명


        // 예: 마우스 커서 따라가기, 배치 미리보기 등 추가 가능
    }

    private void ConfirmPlacement(Vector2 position)
    {
    if (itemToPlace != null && itemToPlace.placeablePrefab != null)
    {
        Instantiate(itemToPlace.placeablePrefab, position, Quaternion.identity);
        UseItem(itemToPlace, 1);
    }

    Destroy(previewInstance);
    previewInstance = null;
    itemToPlace = null;
    isPlacing = false;
}

    private void Update()
    {
        if (isPlacing && Input.GetMouseButtonDown(0))
        {
            Vector2 worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            PlaceItemAt(worldPos);
        }

        if (!isPlacing) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 위치에 미리보기 따라다님
        if (previewInstance != null)
            previewInstance.transform.position = mousePos;

        // 왼쪽 클릭으로 설치 확정
        if (Input.GetMouseButtonDown(0))
        {
            ConfirmPlacement(mousePos);
        }

        // 오른쪽 클릭으로 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }

    private void PlaceItemAt(Vector2 position)
    {
        if (itemToPlace.placeablePrefab != null)
        {
            Instantiate(itemToPlace.placeablePrefab, position, Quaternion.identity);
            UseItem(itemToPlace, 1);
        }

        isPlacing = false;
        itemToPlace = null;
    }

    private void CancelPlacement()
{
    Destroy(previewInstance);
    previewInstance = null;
    itemToPlace = null;
    isPlacing = false;
}
    public void UseItem(ItemData item, int amount = 1)
    {
        InventorySlot_PossessableObject slot = HaveItem.Instance.inventorySlots.Find(s => s.item == item);
        if (slot != null)
        {
            slot.UseItem(amount);

            if (slot.IsEmpty())
            {
                slot.DestroySlotUI(); // 슬롯에 연결된 UI 제거
                HaveItem.Instance.inventorySlots.Remove(slot);   // 슬롯 리스트에서도 제거
            }

            // UpdateUI(); // UI 새로고침 (선택사항: 자동 갱신되면 생략 가능)
        }
    }
}
