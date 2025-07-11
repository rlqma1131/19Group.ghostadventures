using System.Collections.Generic;
using UnityEngine;

public class ChapterEndingManager : Singleton<ChapterEndingManager>
{
    private HashSet<string> collectedFakeMemories = new();
    private HashSet<string> collectedAllCh1Clue = new();
    
    /// <summary>
    /// Update문으로 진엔딩 조건 확인 가능
    /// </summary>
    

    public void CollectCh1Clue(string clueId)
    {
        collectedAllCh1Clue.Add(clueId);
        Debug.Log($"단서 수집: {clueId}");
    }

    public bool AllCh1CluesCollected()
    {
        return collectedAllCh1Clue.Count >= 4;
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

    // [진엔딩] 조건 전체 다 수집했는지 수량 확인
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
