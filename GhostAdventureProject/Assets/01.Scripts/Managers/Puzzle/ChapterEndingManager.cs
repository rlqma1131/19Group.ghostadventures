using System.Collections.Generic;
using UnityEngine;

    /// <summary>
    /// 진엔딩 조건 확인 가능
    /// </summary>
public class ChapterEndingManager : Singleton<ChapterEndingManager>
{
    //private HashSet<string> collectedFakeMemories = new();

    [SerializeField] private bool allCh1CluesCollected; // 인스펙터에서 확인용

    // 단서 수집
    private readonly HashSet<string> collectedAllCh1Clue = new();
    private readonly HashSet<string> collectedAllCh2Clue = new();
    private readonly HashSet<string> collectedAllCh3Clue = new();

    // 스캔된 기억
    private readonly HashSet<string> scannedCh1Memories = new();
    private readonly HashSet<string> scannedCh2Memories = new();
    private readonly HashSet<string> scannedCh3Memories = new();

    public int CurrentCh1ClueCount => collectedAllCh1Clue.Count;
    public int CurrentCh2ClueCount => collectedAllCh2Clue.Count;
    public int CurrentCh3ClueCount => collectedAllCh3Clue.Count;

    public int CurrentCh1MemoryCount => scannedCh1Memories.Count;
    public int CurrentCh2MemoryCount => scannedCh2Memories.Count;
    public int CurrentCh3MemoryCount => scannedCh3Memories.Count;

    public bool AllCh1CluesCollected() => allCh1CluesCollected;


    // 단서 등록
    public void CollectCh1Clue(string clueId)
    {
        if (string.IsNullOrEmpty(clueId)) return;
        collectedAllCh1Clue.Add(clueId);
        allCh1CluesCollected = collectedAllCh1Clue.Count >= 4; // 필요시 값 조정
        Debug.Log($"[ChapterEnding] Ch1 단서 수집: {clueId} (총 {CurrentCh1ClueCount})");
    }

    public void CollectCh2Clue(string clueId)
    {
        if (string.IsNullOrEmpty(clueId)) return;
        collectedAllCh2Clue.Add(clueId);
        Debug.Log($"[ChapterEnding] Ch2 단서 수집: {clueId} (총 {CurrentCh2ClueCount})");
    }

    public void CollectCh3Clue(string clueId)
    {
        if (string.IsNullOrEmpty(clueId)) return;
        collectedAllCh3Clue.Add(clueId);
        Debug.Log($"[ChapterEnding] Ch3 단서 수집: {clueId} (총 {CurrentCh3ClueCount})");
    }

    // (공통) 기억 스캔 등록
    public void RegisterScannedMemory(string memoryId, int chapter)
    {
        if (string.IsNullOrEmpty(memoryId)) return;

        switch (chapter)
        {
            case 1:
                scannedCh1Memories.Add(memoryId);
                Debug.Log($"[ChapterEnding] Ch1 기억 스캔: {memoryId} (총 {CurrentCh1MemoryCount})");
                break;
            case 2:
                scannedCh2Memories.Add(memoryId);
                Debug.Log($"[ChapterEnding] Ch2 기억 스캔: {memoryId} (총 {CurrentCh2MemoryCount})");
                break;
            case 3:
                scannedCh3Memories.Add(memoryId);
                Debug.Log($"[ChapterEnding] Ch3 기억 스캔: {memoryId} (총 {CurrentCh3MemoryCount})");
                break;
            default:
                Debug.LogWarning($"[ChapterEnding] 알 수 없는 챕터({chapter})에 기억 등록 시도: {memoryId}");
                break;
        }
    }


    //public void CollectFakeMemory(string id)
    //{
    //    collectedFakeMemories.Add(id);
    //    Debug.Log($"가짜 기억 수집: {id}");

    //    if (AllFakeMemoriesCollected())
    //    {
    //        UnlockFakeEnding();
    //    }
    //}

    // [진엔딩] 조건 전체 다 수집했는지 수량 확인
    //private bool AllFakeMemoriesCollected()
    //{
    //    return collectedFakeMemories.Count >= 5;
    //}

    //private void UnlockFakeEnding()
    //{
    //    Debug.Log("진엔딩 해금!");
    //    // 트리거 설정, 플래그 저장 등
    //}
}
