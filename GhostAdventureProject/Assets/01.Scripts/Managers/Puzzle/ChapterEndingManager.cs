using System.Collections.Generic;
using UnityEngine;
using static Unity.Collections.AllocatorManager;

public class ChapterEndingManager : Singleton<ChapterEndingManager>
{
    private HashSet<string> collectedFakeMemories = new();
    private HashSet<string> collectedAllCh1Clue = new();

    public void CollectCh1Clue(string clueId)
    {
        collectedAllCh1Clue.Add(clueId);
        Debug.Log($"단서 수집: {clueId}");
        if (AllCh1CluesCollected())
        {
            UnlockCh1GarageEvent();
        }
    }

    public bool AllCh1CluesCollected()
    {
        return collectedAllCh1Clue.Count >= 4;
    }

    private void UnlockCh1GarageEvent()
    {
        Debug.Log("1장 차고 이벤트 해금!");
        //자동 저장 등
    }

    public void CollectFakeMemory(string id)
    {
        collectedFakeMemories.Add(id);
        Debug.Log($"가짜 기억 수집: {id}");

        if (AllFakeMemoriesCollected())
        {
            UnlockFakeEnding();
        }
    }

    // 전체 다 수집했는지 수량 확인
    private bool AllFakeMemoriesCollected()
    {
        return collectedFakeMemories.Count >= 5;
    }

    private void UnlockFakeEnding()
    {
        Debug.Log("진엔딩 해금!");
        // 트리거 설정, 플래그 저장 등
    }
}
