using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Inventory_Player : MonoBehaviour
{
    //관리인 ( 직원증 ( 관리실 문 해금 ) , 포스트잇 ( 메모판에서의 조합 ) , 열쇠 ( 금고 열쇠 ) )
    // 라디오가 켜져서 관리실에서 나와 라디오를 끄러 옴 ( 켠디션 하 )
    //라디오를 끄고 벤치에 앉으러 갈때까지 ( 컨디션 중 )
    //벤치에 앉음 ( 컨디션 상 )



    public List<ClueData> collectedClues = new List<ClueData>(); // 단서데이터를 모아놓은 리스트
    public List<InventorySlot_Player> inventorySlots; // 슬롯 4개
    private int currentPage = 0;
    private int cluesPerPage = 4;
    // [SerializeField] TextMeshProUGUI currentPageText; // 현재 페이지 표시

    public void AddClue(ClueData clue)
    {
        
        if (!collectedClues.Contains(clue)) //같은 clue 중복획득 방지 
        {
            collectedClues.Add(clue);
            //UI 갱신 이벤트 호출
            RefreshUI();
        }
    }

    public void RefreshUI()
    {
        int startIndex = currentPage * cluesPerPage;

        for(int i=0; i<inventorySlots.Count; i++)
        {
            int clueIndex = startIndex + i;
            if(clueIndex < collectedClues.Count)
            {
                inventorySlots[i].Setup(collectedClues[clueIndex]); //5번째 단서까지만 보이게
            }
            else 
            {
                inventorySlots[i].Clear(); //6번째 단서는 보이지 않음
            }
            // currentPageText.text = currentPage.ToString(); //현재 페이지 표시 
        }
    }
    public void NextPage() // 다음 페이지로
    {
        int maxPage = (collectedClues.Count - 1) / cluesPerPage;
        if (currentPage < maxPage)
        {
            currentPage++;
            Debug.Log("다음 페이지: " + currentPage);
            RefreshUI();
        }
    }

    public void PrevPage() // 이전 페이지로
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshUI();
        }
    }

    public void ResetPage() // 처음 페이지로 (아직 미사용)
    {
        currentPage = 0;
        RefreshUI();
    }

    // private void Update()
    // {
    //     for (int i = 1; i <= 5; i++)
    //     {
    //         if (Input.GetKeyDown(KeyCode.Alpha0 + i))
    //         {
    //             int slotIndex = i - 1;
    //             int clueIndex = currentPage * cluesPerPage + slotIndex;

    //             if (InventoryExpandViewer.Instance.IsShowing())
    //             {
    //                 InventoryExpandViewer.Instance.HideClue();
    //             }
    //             else if (clueIndex < collectedClues.Count)
    //             {
    //                 InventoryExpandViewer.Instance.ShowClue(collectedClues[clueIndex]);
    //             }
    //         }
    //     }
    // }


    private void Update()
    {
        for (int i = 0; i < 5; i++)
        {
            KeyCode key = KeyBindingManager.Instance.GetKey(i);
            if (Input.GetKeyDown(key))
            {
                int clueIndex = currentPage * cluesPerPage + i;

                if (InventoryExpandViewer.Instance.IsShowing())
                {
                    InventoryExpandViewer.Instance.HideClue();
                }
                else if (clueIndex < collectedClues.Count)
                {
                    InventoryExpandViewer.Instance.ShowClue(collectedClues[clueIndex]);
                }
            }
        }
    }
}




