using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 챕터별 단서 기록 상태
[System.Serializable]
public class ChapterClueProgress
{
    public int chapter;
    public List<string> clueIds = new();
    public bool allCollected;
}

// 챕터별 기억 기록 상태
[System.Serializable]
public class ChapterMemoryProgress
{
    public int chapter;                 // 1,2,3
    public List<string> memoryIds = new();
}

// 오브젝트 SetActive 상태
[System.Serializable]
public class ActiveObjectState
{
    public string id;
    public bool active;
}

// 오브젝트 위치
[System.Serializable]
public class ObjectPositionState
{
    public string id;
    public Vector3 position;
}

// 빙의오브젝트 활성화 상태
[System.Serializable]
public class PossessableState
{
    public string id;
    public bool hasActivated;
}

// 기억오브젝트 스캔 가능 상태
[System.Serializable]
public class MemoryFragmentState
{
    public string id;
    public bool isScannable;
}

// 문 열림, 잠김 상태
[System.Serializable]
public class DoorState
{
    public string id;
    public bool isLocked; // true=잠김, false=열림
}

// 빙의 대상 인벤토리 상태 저장
public class PossessableInventoryEntry
{
    public string itemKey;   // ItemData의 고유키(권장: 에셋 파일명 item.name)
    public int quantity;
}

[System.Serializable]
public class PossessableInventoryState
{
    public string ownerId;                      // UniqueId.Id
    public List<PossessableInventoryEntry> items = new();
}

// 튜토리얼 진행도 저장용
[System.Serializable]
public class RoomVisitEntry
{
    public string roomName;
    public int count;
}

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public string checkpointId;

    public List<PossessableInventoryState> possessableInventories;
    public List<string> collectedClueNames;
    public List<string> collectedMemoryIDs;
    public List<string> scannedMemoryTitles;
    public List<string> solvedPuzzleIDs;

    public List<ChapterMemoryProgress> chapterMemoryProgress = new();
    public List<ChapterClueProgress> chapterClueProgress = new();

    public List<ActiveObjectState> activeObjectStates;
    public List<ObjectPositionState> objectPositions;
    public List<DoorState> doorStates;
    public List<PossessableState> possessableStates;
    public List<MemoryFragmentState> memoryFragmentStates;

    // 튜토리얼 진행도 저장용
    public List<RoomVisitEntry> roomVisitCounts = new();
}

public static class SaveManager
{
    // ===== 공통 =====
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentData;
    public static SaveData CurrentData => currentData;
    public static event Action<SaveData> Loaded;

    private static void EnsureData()
    {
        if (currentData == null) currentData = new SaveData();
        if (currentData.chapterMemoryProgress == null) currentData.chapterMemoryProgress = new List<ChapterMemoryProgress>();
        if (currentData.chapterClueProgress == null) currentData.chapterClueProgress = new List<ChapterClueProgress>();
        if (currentData.objectPositions == null) currentData.objectPositions = new List<ObjectPositionState>();
        if (currentData.possessableInventories == null) currentData.possessableInventories = new List<PossessableInventoryState>();
        if (currentData.collectedClueNames == null) currentData.collectedClueNames = new List<string>();
        if (currentData.collectedMemoryIDs == null) currentData.collectedMemoryIDs = new List<string>();
        if (currentData.scannedMemoryTitles == null) currentData.scannedMemoryTitles = new List<string>();
        if (currentData.solvedPuzzleIDs == null) currentData.solvedPuzzleIDs = new List<string>();
        if (currentData.activeObjectStates == null) currentData.activeObjectStates = new List<ActiveObjectState>();
        if (currentData.doorStates == null) currentData.doorStates = new List<DoorState>();
        if (currentData.possessableStates == null) currentData.possessableStates = new List<PossessableState>();
        if (currentData.memoryFragmentStates == null) currentData.memoryFragmentStates = new List<MemoryFragmentState>();
    }

    // ===== 챕터 최종단서 수집 진행도 저장/적용 =====
    private static ChapterClueProgress GetOrCreateChapterProgress(int chapter)
    {
        EnsureData();
        var list = currentData.chapterClueProgress;
        int idx = list.FindIndex(x => x.chapter == chapter);
        if (idx >= 0) return list[idx];

        var p = new ChapterClueProgress { chapter = chapter };
        list.Add(p);
        return p;
    }

    // Collect될 때마다 호출할 메서드
    public static void AddChapterClue(int chapter, string clueId, bool allCollected = false, bool autosave = false)
    {
        if (string.IsNullOrEmpty(clueId)) return;
        var p = GetOrCreateChapterProgress(chapter);

        if (!p.clueIds.Contains(clueId))
            p.clueIds.Add(clueId);

        // ChapterEndingManager 쪽 판단 결과를 그대로 반영
        if (allCollected) p.allCollected = true;

        if (autosave) SaveGame();
    }

    // 로드시 ChapterEndingManager가 세트로 복구할 때 사용
    public static bool TryGetChapterClueProgress(int chapter, out ChapterClueProgress progress)
    {
        progress = currentData?.chapterClueProgress?.Find(x => x.chapter == chapter);
        return progress != null;
    }

    public static void SetChapterProgress(int chapter, IEnumerable<string> ids, bool allCollected)
    {
        var p = GetOrCreateChapterProgress(chapter);
        p.clueIds = ids != null ? new List<string>(ids) : new List<string>();
        p.allCollected = allCollected;
    }

    // ===== 챕터 기억 수집 진행도 저장/적용 =====
    private static ChapterMemoryProgress GetOrCreateChapterMemory(int chapter)
    {
        EnsureData();
        var list = currentData.chapterMemoryProgress;
        int idx = list.FindIndex(x => x.chapter == chapter);
        if (idx >= 0) return list[idx];

        var p = new ChapterMemoryProgress { chapter = chapter };
        list.Add(p);
        return p;
    }

    // 스냅샷 세팅
    public static void SetChapterScannedMemories(int chapter, IEnumerable<string> ids)
    {
        var p = GetOrCreateChapterMemory(chapter);
        p.memoryIds = ids != null ? new List<string>(ids) : new List<string>();
    }

    // 로드시 읽기
    public static bool TryGetChapterScannedMemories(int chapter, out ChapterMemoryProgress progress)
    {
        progress = currentData?.chapterMemoryProgress?.Find(x => x.chapter == chapter);
        return progress != null;
    }



    // ===== 튜토리얼 진행도 저장/적용 =====
    public static void SetRoomVisitCount(string roomName, int count)
    {
        EnsureData();
        if (currentData.roomVisitCounts == null) currentData.roomVisitCounts = new List<RoomVisitEntry>();
        int i = currentData.roomVisitCounts.FindIndex(x => x.roomName == roomName);
        if (i >= 0) currentData.roomVisitCounts[i].count = count;
        else currentData.roomVisitCounts.Add(new RoomVisitEntry { roomName = roomName, count = count });
    }

    public static bool TryGetRoomVisitCount(string roomName, out int count)
    {
        count = 0;
        var e = currentData?.roomVisitCounts?.Find(x => x.roomName == roomName);
        if (e == null) return false;
        count = e.count;
        return true;
    }

    // ===== 오브젝트 활성화 상태 저장/적용 =====
    public static void SetActiveState(string id, bool active)
    {
        EnsureData();
        var list = currentData.activeObjectStates;
        int i = list.FindIndex(x => x.id == id);
        if (i >= 0) list[i].active = active;
        else list.Add(new ActiveObjectState { id = id, active = active });
    }

    public static bool TryGetActiveState(string id, out bool active)
    {
        active = true;
        var s = currentData?.activeObjectStates?.Find(x => x.id == id);
        if (s == null) return false;
        active = s.active;
        return true;
    }

    // ===== 오브젝트 위치 저장/적용 =====
    public static void SetObjectPosition(string id, Vector3 pos)
    {
        EnsureData();
        var list = currentData.objectPositions;
        int i = list.FindIndex(x => x.id == id);
        if (i >= 0) list[i].position = pos;
        else list.Add(new ObjectPositionState { id = id, position = pos });
    }
    
    public static bool TryGetObjectPosition(string id, out Vector3 pos)
    {
        pos = default;
        var s = currentData?.objectPositions?.Find(x => x.id == id);
        if (s == null) return false;
        pos = s.position;
        return true;
    }

    // ===== BasePossessable 상태 저장/적용 =====
    public static void SetPossessableState(string id, bool hasActivated)
    {
        EnsureData();
        var list = currentData.possessableStates;
        int i = list.FindIndex(x => x.id == id);
        if (i >= 0) list[i].hasActivated = hasActivated;
        else list.Add(new PossessableState { id = id, hasActivated = hasActivated });
    }

    public static bool TryGetPossessableState(string id, out bool hasActivated)
    {
        hasActivated = true;
        var s = currentData?.possessableStates?.Find(x => x.id == id);
        if (s == null) return false;
        hasActivated = s.hasActivated;
        return true;
    }


    // ===== MemoryFragment 상태 저장/적용 =====
    public static void SetMemoryFragmentScannable(string id, bool isScannable)
    {
        EnsureData();
        var list = currentData.memoryFragmentStates;
        int i = list.FindIndex(x => x.id == id);
        if (i >= 0) list[i].isScannable = isScannable;
        else list.Add(new MemoryFragmentState { id = id, isScannable = isScannable });
    }

    public static bool TryGetMemoryFragmentScannable(string id, out bool isScannable)
    {
        isScannable = false;
        var s = currentData?.memoryFragmentStates?.Find(x => x.id == id);
        if (s == null) return false;
        isScannable = s.isScannable;
        return true;
    }

    // ===== 문 상태 저장/적용 =====
    public static void SetDoorLocked(string id, bool isLocked)
    {
        EnsureData();
        var list = currentData.doorStates;
        int i = list.FindIndex(x => x.id == id);
        if (i >= 0) list[i].isLocked = isLocked;
        else list.Add(new DoorState { id = id, isLocked = isLocked });
    }

    public static bool TryGetDoorLocked(string id, out bool isLocked)
    {
        isLocked = true;
        var s = currentData?.doorStates?.Find(x => x.id == id);
        if (s == null) return false;
        isLocked = s.isLocked;
        return true;
    }

    // ===== 플레이어 인벤토리 상태 적용 =====
    public static void ApplyPlayerInventoryFromSave(Inventory_Player inv)
    {
        if (currentData?.collectedClueNames == null) return;

        inv.RemoveClueBeforeStage(); // 전체 초기화
        foreach (var clueName in currentData.collectedClueNames)
        {
            // 저장한 기준에 맞춰 로드 경로/키 선택
            var clue = Resources.Load<ClueData>("ClueData/" + clueName); // (에셋 파일명 기준)
                                                                         // var clue = /* clue_Name 기준이면 */ Resources.LoadAll<ClueData>("ClueData")
                                                                         //               .FirstOrDefault(c => c.clue_Name == clueName);

            if (clue != null) inv.AddClue(clue);
        }
    }

    // ===== 빙의 대상 인벤토리 저장/조회 =====
    public static void SetPossessableInventory(string ownerId, List<PossessableInventoryEntry> items)
    {
        EnsureData();
        var list = currentData.possessableInventories;
        int i = list.FindIndex(x => x.ownerId == ownerId);
        if (i >= 0) list[i].items = items ?? new List<PossessableInventoryEntry>();
        else list.Add(new PossessableInventoryState { ownerId = ownerId, items = items ?? new List<PossessableInventoryEntry>() });
    }

    public static bool TryGetPossessableInventory(string ownerId, out List<PossessableInventoryEntry> items)
    {
        items = null;
        var s = currentData?.possessableInventories?.Find(x => x.ownerId == ownerId);
        if (s == null) return false;
        items = new List<PossessableInventoryEntry>(s.items ?? new List<PossessableInventoryEntry>());
        return true;
    }
    


    // ===== 조회(읽기) 계열 =====
    public static bool HasSaveFile() => File.Exists(SavePath);

    public static bool IsPuzzleSolved(string puzzleID)
        => currentData?.solvedPuzzleIDs?.Contains(puzzleID) ?? false;

    public static bool HasCollectedClue(string clueName)
        => currentData?.collectedClueNames?.Contains(clueName) ?? false;

    public static bool HasCollectedMemoryID(string memoryID)
        => currentData?.collectedMemoryIDs?.Contains(memoryID) ?? false;

    public static bool HasScannedMemoryTitle(string title)
        => currentData?.scannedMemoryTitles?.Contains(title) ?? false;

    public static string GetLastSceneName()
        => currentData?.sceneName ?? string.Empty;

    public static Vector3? GetLastPlayerPosition()
        => currentData != null ? currentData.playerPosition : (Vector3?)null;

    public static string GetCheckpointId()
        => currentData?.checkpointId ?? string.Empty;


    // ===== 변경(쓰기) 계열 =====
    public static void SaveGame(SaveData data = null)
    {
        if (data != null) currentData = data;
        EnsureData();

        try
        {
            string json = JsonUtility.ToJson(currentData, true);
            string tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tmp, SavePath);

            Debug.Log($"[SaveManager] Saved to: {SavePath}\n" +
                      $" scene={currentData.sceneName}" +
                      $" memIDs={currentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={currentData.collectedClueNames?.Count ?? 0}" +
                      $" bytes={json.Length}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] Save FAILED at {SavePath}\n{ex}");
        }

        // 팝업 Null-safe
        var popup = UIManager.Instance != null ? UIManager.Instance.SaveNoticePopupUI : null;
        if (popup != null) popup.FadeInAndOut("게임이 저장되었습니다.");
    }


    public static SaveData LoadGame()
    {
        Debug.Log($"[SaveManager] Try load from: {SavePath}");
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] No save file.");
            currentData = null;
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            currentData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            EnsureData();

            Debug.Log($"[SaveManager] Loaded. scene={currentData.sceneName}" +
                      $" memIDs={currentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={currentData.collectedClueNames?.Count ?? 0}" +
                      $" bytes={json.Length}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] Load FAILED at {SavePath}\n{ex}");
            currentData = new SaveData();
            EnsureData();
        }

        Loaded?.Invoke(currentData);
        return currentData;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        currentData = null; // 조회 계열은 이후 false/기본값을 반환
    }

    // 퍼즐 풀었을 때 기록
    public static void MarkPuzzleSolved(string puzzleID)
    {
        EnsureData();
        if (!currentData.solvedPuzzleIDs.Contains(puzzleID))
        {
            currentData.solvedPuzzleIDs.Add(puzzleID);
            //if (autosave) SaveGame();
        }
    }

    // 단서 수집했을 때 기록
    public static void AddCollectedClue(string clueName)
    {
        EnsureData();
        if (!currentData.collectedClueNames.Contains(clueName))
        {
            currentData.collectedClueNames.Add(clueName);
            //if (autosave) SaveGame();
        }
    }

    // 기억 스캔했을 때 데이터이름 기록
    public static void AddCollectedMemoryID(string memoryID)
    {
        EnsureData();
        if (!currentData.collectedMemoryIDs.Contains(memoryID))
        {
            currentData.collectedMemoryIDs.Add(memoryID);
            //if (autosave) SaveGame();
        }
    }

    // 기억 스캔했을 때 기억제목 저장
    public static void AddScannedMemoryTitle(string title)
    {
        EnsureData();
        if (!currentData.scannedMemoryTitles.Contains(title))
        {
            currentData.scannedMemoryTitles.Add(title);
            //if (autosave) SaveGame();
        }
    }

    // 플레이어 인벤토리 상태 저장
    public static void SnapshotPlayerInventory(Inventory_Player inv)
    {
        EnsureData();
        currentData.collectedClueNames.Clear();

        // 에셋 파일명으로 저장
        foreach (var c in inv.collectedClues)
            if (c != null) currentData.collectedClueNames.Add(c.name);
    }

    // 현재 씬 이름과 위치 저장
    public static void SetSceneAndPosition(string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    {
        EnsureData();
        currentData.sceneName = sceneName;
        currentData.playerPosition = playerPos;
        currentData.checkpointId = checkpointId;
    }

    // 기억 스캔할 때 호출
    public static void SaveWhenScan(string memoryID, string title,
    string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);

        // 오브젝트 SetAcitve 상태, 위치 저장
        foreach (var p in GameObject.FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, p.gameObject.activeSelf);
                SetObjectPosition(uid.Id, p.transform.position);
            }
        }

        foreach (var m in GameObject.FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, m.gameObject.activeSelf);
                SetObjectPosition(uid.Id, m.transform.position);
            }
        }

        // 문 상태 저장
        foreach (var door in GameObject.FindObjectsOfType<BaseDoor>(true))
        {
            if (door.TryGetComponent(out UniqueId uid))
                SaveManager.SetDoorLocked(uid.Id, door.IsLocked);
        }

        // 플레이어 인벤토리 상태 저장
        var inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SnapshotPlayerInventory(inv);

        // 빙의 대상 인벤토리 상태 저장
        SnapshotAllPossessableInventories();

        var cem = ChapterEndingManager.Instance;
        if (cem != null)
        {
            // 최종단서 진행도 스냅샷
            SetChapterProgress(1, cem.GetClueIds(1), cem.GetAllCollected(1));
            SetChapterProgress(2, cem.GetClueIds(2), cem.GetAllCollected(2));

            // 스캔된 기억 스냅샷
            SetChapterScannedMemories(1, cem.GetScannedMemoryIds(1));
            SetChapterScannedMemories(2, cem.GetScannedMemoryIds(2));
            SetChapterScannedMemories(3, cem.GetScannedMemoryIds(3));
        }

        SetSceneAndPosition(sceneName, playerPos, checkpointId, autosave);

        if (autosave) SaveGame();
    }

    public static void SnapshotAllPossessableInventories()
    {
        foreach (var have in GameObject.FindObjectsOfType<HaveItem>(true))
        {
            if (!have.TryGetComponent(out UniqueId uid)) continue;
            var items = new List<PossessableInventoryEntry>();

            // InventorySlot_PossessableObject -> (itemKey, quantity)
            foreach (var slot in have.inventorySlots)
            {
                if (slot == null || slot.item == null) continue;
                if (slot.quantity <= 0) continue;

                items.Add(new PossessableInventoryEntry
                {
                    itemKey = slot.item.name,   // 권장: 에셋 파일명 사용
                    quantity = slot.quantity
                });
            }

            SaveManager.SetPossessableInventory(uid.Id, items);
        }
    }
}

