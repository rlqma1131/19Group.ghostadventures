// using System.Collections;
// using System.Collections.Generic;
// using UnityEngine;

// public class Inventory_InteractableObject : MonoBehaviour
// {
//     // public GameObject slotPrefab;               // 슬롯 프리팹 (아이콘 + 텍스트)
//     // public Transform slotParent;                // 슬롯들이 붙을 부모 오브젝트

//     // public List<InventorySlot_IO> slots = new();

//     // public void AddItem(ItemData item, int amount = 1)
//     // {
//     //     InventorySlot_IO existingSlot = slots.Find(s => s.item == item);
//     //     if (existingSlot != null)
//     //     {
//     //         existingSlot.AddItem(amount);
//     //     }
//     //     else
//     //     {
//     //         InventorySlot_IO newSlot = new InventorySlot_IO(item, amount, slotPrefab, slotParent);
//     //         slots.Add(newSlot);
//     //     }
//     // }

//     // public void UseItem(ItemData item, int amount = 1)
//     // {
//     //     InventorySlot_IO slot = slots.Find(s => s.item == item);
//     //     if (slot != null)
//     //     {
//     //         slot.UseItem(amount);
//     //         if (slot.IsEmpty())
//     //         {
//     //             slot.DestroySlotUI();
//     //             slots.Remove(slot);
//     //         }
//     //     }
//     // }

//     // public void ClearInventory()
//     // {
//     //     foreach (var slot in slots)
//     //     {
//     //         slot.DestroySlotUI();
//     //     }
//     //     slots.Clear();
//     // }




//     List<HaveItem> haveitems; // 단서데이터를 모아놓은 리스트
//     public List<InventorySlot> inventorySlots; // 슬롯 4개
//     private int currentPage = 0;
//     private int cluesPerPage = 4;
//     // [SerializeField] TextMeshProUGUI currentPageText; // 현재 페이지 표시

//     void Start()
//     {
//         haveitems = FindObjectOfType<HaveItem>();
//     }
//     public void AddClue(ClueData clue)
//     {
        
//         // if (!collectedClues.Contains(clue)) //같은 clue 중복획득 방지 
//         // {
//             collectedClues.Add(clue);
//             // UI 갱신 이벤트 호출
//             RefreshUI();
//         // }
//     }

//     public void RefreshUI()
//     {
//         int startIndex = currentPage * cluesPerPage;

//         for(int i=0; i<inventorySlots.Count; i++)
//         {
//             int clueIndex = startIndex + i;
//             if(clueIndex < collectedClues.Count)
//             {
//                 inventorySlots[i].Setup(collectedClues[clueIndex]); //5번째 단서까지만 보이게
//             }
//             else 
//             {
//                 inventorySlots[i].Clear(); //6번째 단서는 보이지 않음
//             }
//             // currentPageText.text = currentPage.ToString(); //현재 페이지 표시 
//         }
//     }
//     public void NextPage() // 다음 페이지로
//     {
//         int maxPage = (collectedClues.Count - 1) / cluesPerPage;
//         if (currentPage < maxPage)
//         {
//             currentPage++;
//             Debug.Log("다음 페이지: " + currentPage);
//             RefreshUI();
//         }
//     }

//     public void PrevPage() // 이전 페이지로
//     {
//         if (currentPage > 0)
//         {
//             currentPage--;
//             RefreshUI();
//         }
//     }

//     public void ResetPage() // 처음 페이지로 (아직 미사용)
//     {
//         currentPage = 0;
//         RefreshUI();
//     }

//     private void Update()
//     {
//         for (int i = 1; i <= 5; i++)
//         {
//             if (Input.GetKeyDown(KeyCode.Alpha0 + i))
//             {
//                 int slotIndex = i - 1;
//                 int clueIndex = currentPage * cluesPerPage + slotIndex;

//                 if (InventoryExpandViewer.Instance.IsShowing())
//                 {
//                     InventoryExpandViewer.Instance.HideClue();
//                 }
//                 else if (clueIndex < collectedClues.Count)
//                 {
//                     InventoryExpandViewer.Instance.ShowClue(collectedClues[clueIndex]);
//                 }
//             }
//         }
//     }
// }
// }