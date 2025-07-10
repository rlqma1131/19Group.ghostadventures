using System.Collections.Generic;
using UnityEngine;

public class Inventory_PossessableObject : Singleton<Inventory_PossessableObject>
{
    public Transform slotParent; // 슬롯이 생성될 부모 위치
    public GameObject slotPrefab; // 슬롯 프리팹

    private ItemData itemToPlace;
    private bool isPlacing = false;
    private GameObject previewInstance;

    private List<GameObject> spawnedSlots = new List<GameObject>();


    void Start()
    {
        // UIManager.Instance.Inventory_PossessableObjectUI.gameObject.SetActive(false);

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
    
    // slotPrefab을 slotParent에 생성하고 spawnedSlots에 추가함
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

    // Clear넣을 시 정보 저장이 안되려나?
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

    // 그냥 HideInventory하면 되잖아;
    public void CloseInventory()
    {
        HideInventory();
    }

    public void UseOrPlaceItem(ItemData item)
    {
        if(item == null)
        {
            Debug.Log("item이 null임");
            return;
        }
        if (item.Item_Type == ItemType.Consumable)
        {
            // 아이템 사용 로직
            Debug.Log($"사용: {item.Item_Name}");
            UseItem(item, 1);
        }

        else if (item.Item_Type == ItemType.Placeable)
        {
            // 설치 모드 진입
            Debug.Log($"설치 시작: {item.Item_Name}");
            StartPlacingItem(item);
        }
    }
    
    // 설치 아이템 프리팹(반투명) 보여주기
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
        if (!isPlacing) return;

        Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // 마우스 위치에 미리보기 따라다님
        if (previewInstance != null)
            previewInstance.transform.position = mousePos;

        // 왼쪽 클릭으로 설치 확정
        if (Input.GetMouseButtonDown(0))
        {
            ConfirmPlacement(mousePos);
            UseItem(itemToPlace, 1);
        }

        // 오른쪽 클릭으로 취소
        if (Input.GetMouseButtonDown(1))
        {
            CancelPlacement();
        }
    }


    private void CancelPlacement()
    {
        Destroy(previewInstance);
        previewInstance = null;
        itemToPlace = null;
        isPlacing = false;
    }

    public void UseItem(ItemData item, int amount)
    {
        InventorySlot_PossessableObject slot = spawnedSlots
        .ConvertAll(s => s.GetComponent<InventorySlot_PossessableObject>())
        .Find(s => s.item == item);        
        
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
