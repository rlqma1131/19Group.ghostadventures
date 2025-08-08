using System.Collections.Generic;
//using Unity.VisualScripting;
//using Unity.VisualScripting.ReorderableList;
using UnityEngine;
using UnityEngine.UI;

public class Inventory_PossessableObject : MonoBehaviour
{
    // 싱글톤
    public static Inventory_PossessableObject Instance { get; private set; }

    public Transform slotParent; // 슬롯이 생성될 부모 위치
    public GameObject slotPrefab; // 슬롯 프리팹
    private ItemData itemToPlace;
    private bool isPlacing = false;
    private GameObject previewInstance;
    private List<GameObject> spawnedSlots = new List<GameObject>();
    public InventorySlot_PossessableObject selectedSlot = null; // 선택된 아이템
    [SerializeField] private Inventory_Player inventory_Player;
    private int selectedSlotIndex = -1;

    void Start()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        inventory_Player = FindObjectOfType<Inventory_Player>();
    }

    // 미사용(보류)
    BasePossessable FindPossessed()
    {
        BasePossessable[] all = FindObjectsOfType<BasePossessable>();
        foreach (var obj in all)
        {
            if (obj.isPossessed)
                Debug.Log("obj:" + obj);
                return obj;
        }
        return null;
    }
    
    // slotPrefab을 slotParent에 생성하고 spawnedSlots에 추가함
    public void ShowInventory(List<InventorySlot_PossessableObject> slots)
    {
        // Clear();

        // foreach (var slot in slots)
        // {
        //     GameObject obj = Instantiate(slotPrefab, slotParent);
        //     obj.GetComponent<InventorySlot_PossessableObject>().SetSlot(slot);
        //     spawnedSlots.Add(obj);
        // }
        // gameObject.SetActive(true);

        Clear();

        for (int i = 0; i < slots.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            var slotComponent = obj.GetComponent<InventorySlot_PossessableObject>();
            slotComponent.SetSlot(slots[i]);

            // 🔢 KeyCode 숫자 (예: 5,6,7...)
            int keyNumber = 5 + i;
            if (slotComponent.keyText_PO != null)
            {
                slotComponent.keyText_PO.text = keyNumber.ToString();
                slotComponent.keyText_PO.gameObject.SetActive(true);
            }

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

    public void Clear()
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
            UseItem(item, 0);
        }

        else if (item.Item_Type == ItemType.Placeable)
        {
            // 설치 모드 진입
            Debug.Log($"설치 시작: {item.Item_Name}");
            StartPlacingItem(item);
        }
        if(item.Item_Type == ItemType.Clue)
        {
            inventory_Player.AddClue(item.clue);
            UseItem(item, 0);
            UIManager.Instance.InventoryExpandViewerUI.ShowClue(item.clue);
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
        // if (!isPlacing) return;

        // Vector2 mousePos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

        // // 마우스 위치에 미리보기 따라다님
        // if (previewInstance != null)
        //     previewInstance.transform.position = mousePos;

        // // 왼쪽 클릭으로 설치 확정
        // if (Input.GetMouseButtonDown(0))
        // {
        //     ConfirmPlacement(mousePos);
        //     UseItem(itemToPlace, 1);
        // }

        // // 오른쪽 클릭으로 취소
        // if (Input.GetMouseButtonDown(1))
        // {
        //     CancelPlacement();
        // }

        if (Input.GetKeyDown(KeyCode.Alpha5) || Input.GetKeyDown(KeyCode.Keypad5))
            SelectSlot(0);

        if (Input.GetKeyDown(KeyCode.Alpha6) || Input.GetKeyDown(KeyCode.Keypad6))
            SelectSlot(1);
            
        if (Input.GetKeyDown(KeyCode.Alpha7) || Input.GetKeyDown(KeyCode.Keypad7))
            SelectSlot(2);

        // if (Input.GetKeyDown(KeyCode.Q))
        //     TryUseSelectedItem();
    }

    private void CancelPlacement()
    {
        Destroy(previewInstance);
        previewInstance = null;
        itemToPlace = null;
        isPlacing = false;
    }

    public void SelectSlot(int index)
    {
        if (selectedSlotIndex == index)
        {
            selectedSlotIndex = -1;
            selectedSlot = null;
            HighlightSlot(-1); // 전체 선택 해제
            Debug.Log("슬롯 선택 해제됨");
            return;
        }

        if (index >= 0 && index < spawnedSlots.Count)
        {
            selectedSlotIndex = index;
            selectedSlot = spawnedSlots[index].GetComponent<InventorySlot_PossessableObject>();

            // 선택된 슬롯 시각적 강조 (옵션)
            HighlightSlot(index);
        }
        if(selectedSlot.item != null && selectedSlot.item.Item_Type == ItemType.Clue)
        {
            UseOrPlaceItem(selectedSlot.item);
        }
    }

    private void HighlightSlot(int index)
    {
        for (int i = 0; i < spawnedSlots.Count; i++)
        {
            GameObject slotObj = spawnedSlots[i];
            Outline outline = slotObj.GetComponent<Outline>();

            if (outline != null)
            {
                outline.enabled = (i == index);
            }
        }
    }
    public void TryUseSelectedItem()
    {
        if (selectedSlot == null || selectedSlot.item == null) return;

        // 예: 상호작용 가능한 조건 검사
        if (CanUseItem(selectedSlot.item))
        {
            UseOrPlaceItem(selectedSlot.item);
        }
        else
        {
            Debug.Log("사용 조건이 충족되지 않음");
        }
    }

    private bool CanUseItem(ItemData item)
    {
        if (item == null) return false;

        // 예: 플레이어가 근처에 상호작용 대상이 있는가?
        if (item.Item_Type == ItemType.Consumable)
        {
            // return IsNearUsableTarget(); // 예: 문, 상자, 장치 등
        }

        // 예: 설치형 아이템은 설치 가능한 위치인지?
        if (item.Item_Type == ItemType.Placeable)
        {
            // return IsPlaceableHere(); // 예: 빈 공간인지, 겹치지 않는지
        }

        return true;
    }



    
    public void UseItem(ItemData item, int amount)
    {
        InventorySlot_PossessableObject slot = spawnedSlots
        .ConvertAll(s => s.GetComponent<InventorySlot_PossessableObject>())
        .Find(s => s.item == item);        
        
        if (slot != null)
        {
            slot.UseItem(amount);

            if (slot.IsEmpty() && HaveItem.Instance.inventorySlots != null)
            {
                slot.DestroySlotUI(); // 슬롯에 연결된 UI 제거
                HaveItem.Instance.inventorySlots.Remove(slot);
                HaveItem.Instance.inventorySlots = null;   // 슬롯 리스트에서도 제거
            }

            // UpdateUI(); // UI 새로고침 (선택사항: 자동 갱신되면 생략 가능)
        }
    }

    public void ClearAllSlotHighlights()
{
    foreach (GameObject slotObj in spawnedSlots)
    {
        var slot = slotObj.GetComponent<InventorySlot_PossessableObject>();
        if (slot != null)
            slot.keyText_PO.gameObject.SetActive(false);
    }

    selectedSlot = null;
    selectedSlotIndex = -1;
}

    public ItemData selectedItem() => selectedSlot != null ? selectedSlot.item : null;
}
