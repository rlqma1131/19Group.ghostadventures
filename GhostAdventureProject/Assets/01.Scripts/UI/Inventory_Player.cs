using System.Collections.Generic;
using UnityEngine;

public class Inventory_Player : MonoBehaviour
{
    public List<ClueData> collectedClues = new List<ClueData>(); // 단서데이터를 모아놓은 리스트
    public List<InventorySlot_Player> inventorySlots; // 슬롯 4개
    private int currentPage = 0;
    private int cluesPerPage = 4;
    // [SerializeField] TextMeshProUGUI currentPageText; // 현재 페이지 표시

    public void AddClue(ClueData clue)
    {
        if(clue == null) return;
        // 그림일기라면 기존 것 교체
        if (clue.clueType == ClueType.DrawingClue)
        {
            for (int i = 0; i < collectedClues.Count; i++)
            {
                if (collectedClues[i].clueType == ClueType.DrawingClue)
                {
                    collectedClues[i] = clue;
                    RefreshUI();
                    return;
                }
            }

            // 기존 그림일기 없으면 추가
            collectedClues.Add(clue);
            RefreshUI();
            return;
        }
        
        if (!collectedClues.Contains(clue)) //같은 clue 중복획득 방지 
        {
            collectedClues.Add(clue);
            //UI 갱신 이벤트 호출
            RefreshUI();
        }
    }
    
    public void RemoveCluesByStage(ClueStage stage)
    {
        collectedClues.RemoveAll(clue => clue.clue_Stage == stage);
        RefreshUI();
    }

    public void RemoveClueBeforeStage()
    {
        if(collectedClues != null)
        {
            collectedClues.Clear();
            RefreshUI();
        }
    }

    // 단서 삭제
    public void RemoveClue(ClueData clue)
    {
        if (collectedClues.Contains(clue))
        {
            collectedClues.Remove(clue);
            RefreshUI();
        }
        else
        {
            UIManager.Instance.PromptUI.ShowPrompt("이 단서가 아니야", 2f);
        }
    }
    public void RemoveClue(ClueData[] clues)
    {
        foreach(ClueData clue in clues)
        {
            if(collectedClues.Contains(clue))
            {
                collectedClues.Remove(clue);
            }
        }
        RefreshUI();
    }

    public void RefreshUI()
    {
        int startIndex = currentPage * cluesPerPage;

        for(int i=0; i<inventorySlots.Count; i++)
        {
            int clueIndex = startIndex + i;
            if(clueIndex < collectedClues.Count)
            {
                inventorySlots[i].Setup(collectedClues[clueIndex]); //4번째 단서까지만 보이게
            }
            else 
            {
                inventorySlots[i].Clear(); //5번째 단서는 보이지 않음
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
    //     for (int i = 0; i < 5; i++)
    //     {
    //         if(ESCMenu.Instance != null)
    //         {
    //         KeyCode key = ESCMenu.Instance.GetKey(i);
    //         if (Input.GetKeyDown(key))
    //         {
    //             int clueIndex = currentPage * cluesPerPage + i;

    //             if (InventoryExpandViewer.Instance.IsShowing())
    //             {
    //                 InventoryExpandViewer.Instance.HideClue();
    //             }
    //             else if (clueIndex < collectedClues.Count)
    //             {
    //                 InventoryExpandViewer.Instance.ShowClue(collectedClues[clueIndex]);
    //             }
    //         }}
    //     }
    // }


    for (int i = 0; i < 4; i++)
    {
        KeyCode alphaKey = KeyCode.Alpha1 + i;
        KeyCode keypadKey = KeyCode.Keypad1 + i;

        if (Input.GetKeyDown(alphaKey) || Input.GetKeyDown(keypadKey))
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


