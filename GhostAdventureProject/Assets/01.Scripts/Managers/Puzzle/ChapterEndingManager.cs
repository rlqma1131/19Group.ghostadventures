using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ChapterEndingManager : Singleton<ChapterEndingManager>
{
    [SerializeField] private bool allCh1CluesCollected;

    private readonly HashSet<string> collectedAllCh1Clue = new();
    private readonly HashSet<string> collectedAllCh2Clue = new();

    private readonly HashSet<string> scannedCh1Memories = new();
    private readonly HashSet<string> scannedCh2Memories = new();
    private readonly HashSet<string> scannedCh3Memories = new();

    // 수량 읽기 전용
    public int CurrentCh1ClueCount => collectedAllCh1Clue.Count;
    public int CurrentCh2ClueCount => collectedAllCh2Clue.Count;

    public int CurrentCh1MemoryCount => scannedCh1Memories.Count;
    public int CurrentCh2MemoryCount => scannedCh2Memories.Count;
    public int CurrentCh3MemoryCount => scannedCh3Memories.Count;

    public bool AllCh1CluesCollected() => allCh1CluesCollected;

    // SaveManager에서 읽기 전용
    public IEnumerable<string> GetClueIds(int chapter) => chapter switch
    {
        1 => collectedAllCh1Clue,
        2 => collectedAllCh2Clue,
        _ => System.Array.Empty<string>()
    };

    public bool GetAllCollected(int chapter) =>
        chapter == 1 ? allCh1CluesCollected : false;

    public IEnumerable<string> GetScannedMemoryIds(int chapter) => chapter switch
    {
        1 => scannedCh1Memories,
        2 => scannedCh2Memories,
        3 => scannedCh3Memories,
        _ => System.Array.Empty<string>()
    };

    public event System.Action ProgressChanged;
    private bool appliedFromSaveOnce = false;

    // --- 저장에서 복구 (로드 직후/씬 진입시) ---
    private void OnEnable() { SaveManager.Loaded += OnSaveLoaded; }
    private void OnDisable() { SaveManager.Loaded -= OnSaveLoaded; }

    private void OnSaveLoaded(SaveData data)
    {
        ApplyFromSave();

        var saved = SaveManager.CurrentData;
        var e = saved?.chapterClueProgress?.Find(x => x.chapter == 1);
        Debug.Log($"[LOAD] ch1 entry? {(e != null)} / count={(e?.clueIds?.Count ?? -1)}");
    }

    // 단서 수집 될 때마다 호출되는 메서드
    public void CollectCh1Clue(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (collectedAllCh1Clue.Add(id))
        {
            allCh1CluesCollected = collectedAllCh1Clue.Count >= 4; // 필요시 상수/Serialized로
            ProgressChanged?.Invoke();
        }
    }

    public void CollectCh2Clue(string id)
    {
        if (string.IsNullOrEmpty(id)) return;
        if (collectedAllCh2Clue.Add(id))
            ProgressChanged?.Invoke();
    }

    public void RegisterScannedMemory(string memoryId, int chapter)
    {
        if (string.IsNullOrEmpty(memoryId)) return;
        bool added = false;
        switch (chapter)
        {
            case 1: added = scannedCh1Memories.Add(memoryId); break;
            case 2: added = scannedCh2Memories.Add(memoryId); break;
            case 3: added = scannedCh3Memories.Add(memoryId); break;
        }
        if (added) ProgressChanged?.Invoke();
    }

    public void ApplyFromSave()
    {
        if (appliedFromSaveOnce) return;
        appliedFromSaveOnce = true;

        // Ch1 단서
        if (SaveManager.TryGetChapterClueProgress(1, out var p1) && p1.clueIds != null && p1.clueIds.Count > 0)
        {
            Debug.Log($"ChapterEndingManager : 지워진 수집단서들 {collectedAllCh1Clue}");
            collectedAllCh1Clue.Clear();
            foreach (var id in p1.clueIds) collectedAllCh1Clue.Add(id);
            allCh1CluesCollected = p1.allCollected;
            Debug.Log($"ChapterEndingManager : 로드된 수집단서들 {collectedAllCh1Clue}");
        }
        // Ch2 단서
        if (SaveManager.TryGetChapterClueProgress(2, out var p2) && p2.clueIds != null && p2.clueIds.Count > 0)
        {
            collectedAllCh2Clue.Clear();
            foreach (var id in p2.clueIds) collectedAllCh2Clue.Add(id);
        }

        // 스캔 기억 복구 (저장본에 있을 때만)
        if (SaveManager.TryGetChapterScannedMemories(1, out var m1) && m1.memoryIds != null && m1.memoryIds.Count > 0)
        {
            scannedCh1Memories.Clear();
            foreach (var id in m1.memoryIds) scannedCh1Memories.Add(id);
        }
        if (SaveManager.TryGetChapterScannedMemories(2, out var m2) && m2.memoryIds != null && m2.memoryIds.Count > 0)
        {
            scannedCh2Memories.Clear();
            foreach (var id in m2.memoryIds) scannedCh2Memories.Add(id);
        }
        if (SaveManager.TryGetChapterScannedMemories(3, out var m3) && m3.memoryIds != null && m3.memoryIds.Count > 0)
        {
            scannedCh3Memories.Clear();
            foreach (var id in m3.memoryIds) scannedCh3Memories.Add(id);
        }

        ProgressChanged?.Invoke();
    }


    //public void ApplyFromSave(SaveData data, MemoryManager mm = null)
    //{
    //    if (data == null) return;

    //    int cur = GetCurrentSceneChapter();

    //    // --- 기억(Scanned Memories): 저장에 값이 있을 때 + 현재 챕터만 재구성 ---
    //    if (data.collectedMemoryIDs != null && data.collectedMemoryIDs.Count > 0 && cur != 0)
    //    {
    //        switch (cur)
    //        {
    //            case 1: scannedCh1Memories.Clear(); break;
    //            case 2: scannedCh2Memories.Clear(); break;
    //            case 3: scannedCh3Memories.Clear(); break;
    //        }

    //        foreach (var id in data.collectedMemoryIDs)
    //        {
    //            // 스캔된 메모리들 기록
    //            RegisterScannedMemory(id, cur);
    //        }
    //    }

    //    // --- 단서(Clues)---
    //    if (data.collectedClueNames != null && data.collectedClueNames.Count > 0 && cur != 0)
    //    {
    //        switch (cur)
    //        {
    //            case 1: collectedAllCh1Clue.Clear(); allCh1CluesCollected = false; break;
    //            case 2: collectedAllCh2Clue.Clear(); break;
    //            case 3: collectedAllCh3Clue.Clear(); break;
    //        }

    //        foreach (var clueName in data.collectedClueNames)
    //        {
    //            if (cur == 1) CollectCh1Clue(clueName);
    //            else if (cur == 2) CollectCh2Clue(clueName);
    //            else CollectCh3Clue(clueName);
    //        }
    //    }
    //}

    //// ===== 현재 씬 기반으로 분류 =====
    //private int GetCurrentSceneChapter()
    //{
    //    var name = SceneManager.GetActiveScene().name;
    //    if (string.IsNullOrEmpty(name)) return 0;

    //    var s = name.ToLowerInvariant();
    //    if (s.Contains("ch01")) return 1;
    //    if (s.Contains("ch02")) return 2;
    //    if (s.Contains("ch03")) return 3;
    //    return 0;
    //}
}
