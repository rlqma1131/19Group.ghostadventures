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

// 빙의오브젝트 hasActivated 상태
[System.Serializable]
public class PossessableState
{
    public string id;
    public bool hasActivated;
}

// 기억오브젝트 isScannable 상태
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

// 애니메이터 재생 클립 상태
[System.Serializable]
public class AnimatorClipLayerState
{
    public int layer;
    public string clipName;    // 현재 재생 중인 첫 번째 클립 이름
    public int stateHash;      // 현재 스테이트(fullPathHash)
    public float normalizedTime;
}

[System.Serializable]
public class AnimatorClipSnapshot
{
    public string id;  // UniqueId
    public List<AnimatorClipLayerState> layers = new();
}

// 현재 상태를 담는 레이어 단위 스냅샷
[System.Serializable]
public class AnimatorChildLayerState
{
    public int layer;
    public int fullPathHash;
    public int shortNameHash;
    public float normalizedTime;
    public string clipName; // 디버깅/확인용
}

// UniqueId(부모) 아래 특정 자식 Animator 하나에 대한 스냅샷
[System.Serializable]
public class AnimatorChildSnapshot
{
    public string childPath;
    public List<AnimatorChildLayerState> layers = new();
}

// UniqueId 단위 트리 스냅샷(= 부모 1개 + 그 하위 모든 Animator)
[System.Serializable]
public class AnimatorTreeSnapshot
{
    public string id; // UniqueId.Id
    public List<AnimatorChildSnapshot> animators = new();
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

    public List<string> collectedClueNames;
    public List<string> collectedMemoryIDs;
    public List<string> scannedMemoryTitles;
    public List<string> solvedPuzzleIDs;

    // 애니메이터 상태 스냅샷
    public List<AnimatorClipSnapshot> animatorClips = new();
    public List<AnimatorTreeSnapshot> animatorTrees = new();

    // 챕터별 진행도 저장
    public List<ChapterMemoryProgress> chapterMemoryProgress = new();
    public List<ChapterClueProgress> chapterClueProgress = new();

    // UniqueID를 갖고 있는 오브젝트 활성화 상태 스냅샷
    public List<ActiveObjectState> activeObjectStates;
    public List<ObjectPositionState> objectPositions;
    public List<DoorState> doorStates;
    public List<PossessableState> possessableStates;
    public List<MemoryFragmentState> memoryFragmentStates;

    // 튜토리얼 진행도 저장용
    public List<RoomVisitEntry> roomVisitCounts = new();
    public List<TutorialStep> completedTutorialSteps = new();
}

public static class SaveManager
{
    public static bool VerboseAnimatorLogging = true;
    public static string GetRelativePath(Transform root, Transform target)
    {
        if (root == null || target == null) return null;
        var stack = new System.Collections.Generic.Stack<string>();
        var t = target;
        while (t != null && t != root)
        {
            stack.Push(t.name);
            t = t.parent;
        }
        if (t != root) return null; // target이 root 하위가 아님
        var sb = new System.Text.StringBuilder();
        while (stack.Count > 0)
        {
            if (sb.Length > 0) sb.Append('/');
            sb.Append(stack.Pop());
        }
        return sb.ToString();
    }

    public static Transform FindChildByPath(Transform root, string path)
    {
        if (root == null || string.IsNullOrEmpty(path)) return null;
        // Transform.Find("A/B/C")는 비활성 자식도 찾음
        return root.Find(path);
    }

    // 저장된 AnimatorClipSnapshot들을 보기 좋은 형태로 찍어주는 유틸
    public static void DumpAnimatorClipSnapshots(string header = "[Dump]", int maxObjects = 20, int maxLayers = 4)
    {
        int count = CurrentData?.animatorClips?.Count ?? 0;

        if (count == 0) return;

        int printed = 0;
        foreach (var snap in CurrentData.animatorClips)
        {
            if (snap == null) continue;
            int lc = Mathf.Min(snap.layers?.Count ?? 0, maxLayers);
            for (int i = 0; i < lc; i++)
            {
                var l = snap.layers[i];
            }
            printed++;
        }
    }
    // ===== 공통 =====
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentData;
    public static SaveData CurrentData => currentData;
    public static event Action<SaveData> Loaded;

    private static void EnsureData()
    {
        if (currentData == null) currentData = new SaveData();
        if (currentData.collectedClueNames == null) currentData.collectedClueNames = new List<string>();
        if (currentData.collectedMemoryIDs == null) currentData.collectedMemoryIDs = new List<string>();
        if (currentData.scannedMemoryTitles == null) currentData.scannedMemoryTitles = new List<string>();
        if (currentData.solvedPuzzleIDs == null) currentData.solvedPuzzleIDs = new List<string>();
        if (currentData.chapterMemoryProgress == null) currentData.chapterMemoryProgress = new List<ChapterMemoryProgress>();
        if (currentData.chapterClueProgress == null) currentData.chapterClueProgress = new List<ChapterClueProgress>();
        if (currentData.objectPositions == null) currentData.objectPositions = new List<ObjectPositionState>();
        if (currentData.animatorClips == null) currentData.animatorClips = new List<AnimatorClipSnapshot>();
        if (currentData.animatorTrees == null) currentData.animatorTrees = new List<AnimatorTreeSnapshot>();
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

    // 기억 스냅샷 세팅
    public static void SetChapterScannedMemories(int chapter, IEnumerable<string> ids)
    {
        var p = GetOrCreateChapterMemory(chapter);
        p.memoryIds = ids != null ? new List<string>(ids) : new List<string>();
    }

    public static bool TryGetChapterScannedMemories(int chapter, out ChapterMemoryProgress progress)
    {
        progress = currentData?.chapterMemoryProgress?.Find(x => x.chapter == chapter);
        return progress != null;
    }

    // 애니메이터 읽고 쓰기
    public static void SetAnimatorClips(string id, AnimatorClipSnapshot snap)
    {
        EnsureData();
        int i = currentData.animatorClips.FindIndex(x => x.id == id);
        if (i >= 0) currentData.animatorClips[i] = snap;
        else currentData.animatorClips.Add(snap);
    }

    public static bool TryGetAnimatorClips(string id, out AnimatorClipSnapshot snap)
    {
        snap = currentData?.animatorClips?.Find(x => x.id == id);
        return snap != null;
    }
    public static void SetAnimatorTree(string id, AnimatorTreeSnapshot snap)
    {
        EnsureData();
        int i = currentData.animatorTrees.FindIndex(x => x.id == id);
        if (i >= 0) currentData.animatorTrees[i] = snap;
        else currentData.animatorTrees.Add(snap);
    }

    public static bool TryGetAnimatorTree(string id, out AnimatorTreeSnapshot snap)
    {
        snap = currentData?.animatorTrees?.Find(x => x.id == id);
        return snap != null;
    }

    // ===== 튜토리얼 진행도 저장/적용 =====
    // 방문횟수
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

    // 튜토리얼 완료 목록 상태
    public static void SetCompletedTutorialSteps(IEnumerable<TutorialStep> steps)
    {
        EnsureData();
        currentData.completedTutorialSteps = steps != null
            ? new List<TutorialStep>(steps)
            : new List<TutorialStep>();
    }

    public static bool TryGetCompletedTutorialSteps(out List<TutorialStep> steps)
    {
        steps = currentData?.completedTutorialSteps;
        return steps != null && steps.Count > 0;
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

    /// UniqueID를 갖고 있는 오브젝트 활성화 상태 스냅샷
    private static void SnapshotAllUniqueIdActivesAndPositions()
    {
        // true: 비활성 포함, 모든 로드된 씬 대상
        var uids = GameObject.FindObjectsOfType<UniqueId>(true);

        foreach (var uid in uids)
        {
            var go = uid.gameObject;

            // DontDestroyOnLoad에 있는 전역 오브젝트는 스킵 (옵션)
            var scn = go.scene;
            if (!scn.IsValid() || !scn.isLoaded || scn.name == "DontDestroyOnLoad")
                continue;

            Debug.Log($"[SaveManager] 오브젝트 : {uid.Id}, 활성화 상태 : {go.activeSelf}");

            // 저장
            SetActiveState(uid.Id, go.activeSelf);
            SetObjectPosition(uid.Id, go.transform.position);
        }
    }

    // ===== 애니메이터 상태 스냅샷 =====

    public static void SnapshotAllAnimatorTrees()
    {
        var uids = GameObject.FindObjectsOfType<UniqueId>(true);

        foreach (var uid in uids)
        {
            var root = uid.transform;
            var anims = root.GetComponentsInChildren<Animator>(true);
            if (anims == null || anims.Length == 0) continue;

            var tree = new AnimatorTreeSnapshot { id = uid.Id };
            var sb = new System.Text.StringBuilder();
            sb.Append($"[SNAP-TREE] uid={uid.Id} go='{uid.gameObject.name}' animators={anims.Length}");

            foreach (var anim in anims)
            {
                var childPath = GetRelativePath(root, anim.transform);
                if (string.IsNullOrEmpty(childPath)) continue;

                var childSnap = new AnimatorChildSnapshot { childPath = childPath };

                int lc = anim.layerCount;
                for (int l = 0; l < lc; l++)
                {
                    var info = anim.GetCurrentAnimatorStateInfo(l);

                    string clipName = "";
                    var clips = anim.GetCurrentAnimatorClipInfo(l);
                    if (clips != null && clips.Length > 0 && clips[0].clip != null)
                        clipName = clips[0].clip.name;

                    childSnap.layers.Add(new AnimatorChildLayerState
                    {
                        layer = l,
                        fullPathHash = info.fullPathHash,
                        shortNameHash = info.shortNameHash,
                        normalizedTime = info.normalizedTime,
                        clipName = clipName
                    });
                }

                tree.animators.Add(childSnap);
                sb.Append($"\n  - path='{childPath}' layers={childSnap.layers.Count}");
                foreach (var ly in childSnap.layers)
                    sb.Append($" | L{ly.layer}: full={ly.fullPathHash} short={ly.shortNameHash} norm={ly.normalizedTime:F3} clip='{ly.clipName}'");
            }

            SetAnimatorTree(uid.Id, tree);
        }
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

            int animClipCount = currentData.animatorClips?.Count ?? 0;

            Debug.Log($"[SaveManager] Saved to: {SavePath}\n" +
                      $" scene={currentData.sceneName}" +
                      $" memIDs={currentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={currentData.collectedClueNames?.Count ?? 0}" +
                      $" animClips={animClipCount}" +     // <-- 추가
                      $" bytes={json.Length}");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveManager] Save FAILED at {SavePath}\n{ex}");
        }

        var popup = UIManager.Instance != null ? UIManager.Instance.SaveNoticePopupUI : null;
        if (popup != null) popup.FadeInAndOut("게임이 저장되었습니다.");
    }


    // ===== 변경(쓰기) 계열 =====
    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            currentData = null;
            return null;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            currentData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
            EnsureData();

            int animClipCount = currentData.animatorClips?.Count ?? 0;

            Debug.Log($"[SaveManager] Loaded. scene={currentData.sceneName}" +
                      $" memIDs={currentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={currentData.collectedClueNames?.Count ?? 0}" +
                      $" animClips={animClipCount}" +     // <-- 추가
                      $" bytes={json.Length}");
        }
        catch (System.Exception)
        {
            currentData = new SaveData();
            EnsureData();
        }

        // 로드된 내용 요약 덤프
        DumpAnimatorClipSnapshots("[LOAD] snapshot summary", maxObjects: 10, maxLayers: 2);

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

    // 기억 스캔 ScanedCheck 할 때 호출
    public static void SaveWhenScanAfter(string memoryID, string title,
    string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);
        SnapshotAllUniqueIdActivesAndPositions();

        // 오브젝트 SetAcitve 상태, 위치 저장
        foreach (var p in GameObject.FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, p.gameObject.activeSelf);
                SetObjectPosition(uid.Id, p.transform.position);

                SetPossessableState(uid.Id, p.HasActivated);
            }
        }

        foreach (var m in GameObject.FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, m.gameObject.activeSelf);
                SetObjectPosition(uid.Id, m.transform.position);

                SetMemoryFragmentScannable(uid.Id, m.IsScannable);
            }
        }
        
        // === Ch2_DrawingClue 상태 저장 추가 ===
        foreach (var clue in GameObject.FindObjectsOfType<Ch2_DrawingClue>(true))
        {
            if (clue.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, clue.gameObject.activeSelf);
                SetObjectPosition(uid.Id, clue.transform.position);
                SetPossessableState(uid.Id, clue.HasActivated); // <-- hasActivated 저장
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
        //SnapshotAllPossessableInventories();

        var cem = ChapterEndingManager.Instance;
        if (cem != null)
        {
            Debug.Log($"[SAVE] ch1 clues now={cem.CurrentCh1ClueCount}");

            // 최종단서 진행도 스냅샷
            SetChapterProgress(1, cem.GetClueIds(1), cem.GetAllCollected(1));
            SetChapterProgress(2, cem.GetClueIds(2), cem.GetAllCollected(2));

            // 스캔된 기억 스냅샷
            SetChapterScannedMemories(1, cem.GetScannedMemoryIds(1));
            SetChapterScannedMemories(2, cem.GetScannedMemoryIds(2));
            SetChapterScannedMemories(3, cem.GetScannedMemoryIds(3));
        }

        SetSceneAndPosition(sceneName, playerPos, checkpointId, autosave);
        // 애니메이터 상태 스탭샷
        SnapshotAllAnimatorTrees();
        DumpAnimatorClipSnapshots("[SAVE] snapshot summary", maxObjects: 10, maxLayers: 2);

        if (autosave) SaveGame();
    }


    public static void SaveWhenCutScene(string memoryID, string title,
    string sceneName, string checkpointId = null, bool autosave = true)
    {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);
        SnapshotAllUniqueIdActivesAndPositions();

        // 오브젝트 SetAcitve 상태, 위치 저장
        foreach (var p in GameObject.FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, p.gameObject.activeSelf);
                SetObjectPosition(uid.Id, p.transform.position);

                SetPossessableState(uid.Id, p.HasActivated);
            }
        }

        foreach (var m in GameObject.FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid))
            {
                SetActiveState(uid.Id, m.gameObject.activeSelf);
                SetObjectPosition(uid.Id, m.transform.position);

                SetMemoryFragmentScannable(uid.Id, m.IsScannable);
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
        //SnapshotAllPossessableInventories();

        var cem = ChapterEndingManager.Instance;
        if (cem != null)
        {
            Debug.Log($"[SAVE] ch1 clues now={cem.CurrentCh1ClueCount}");

            // 최종단서 진행도 스냅샷
            SetChapterProgress(1, cem.GetClueIds(1), cem.GetAllCollected(1));
            SetChapterProgress(2, cem.GetClueIds(2), cem.GetAllCollected(2));

            // 스캔된 기억 스냅샷
            SetChapterScannedMemories(1, cem.GetScannedMemoryIds(1));
            SetChapterScannedMemories(2, cem.GetScannedMemoryIds(2));
            SetChapterScannedMemories(3, cem.GetScannedMemoryIds(3));
        }

        // 애니메이터 상태 스탭샷
        SnapshotAllAnimatorTrees();
        DumpAnimatorClipSnapshots("[SAVE] snapshot summary", maxObjects: 10, maxLayers: 2);

        if (autosave) SaveGame();
    }

    //public static void SnapshotAllPossessableInventories()
    //{
    //    foreach (var have in GameObject.FindObjectsOfType<HaveItem>(true))
    //    {
    //        if (!have.TryGetComponent(out UniqueId uid)) continue;
    //        //var items = new List<PossessableInventoryEntry>();

    //        // InventorySlot_PossessableObject -> (itemKey, quantity)
    //        //foreach (var slot in have.inventorySlots)
    //        //{
    //        //    if (slot == null || slot.item == null) continue;
    //        //    if (slot.quantity <= 0) continue;

    //        //    items.Add(new PossessableInventoryEntry
    //        //    {
    //        //        itemKey = slot.item.name,   // 권장: 에셋 파일명 사용
    //        //        quantity = slot.quantity
    //        //    });
    //        //}

    //        //SaveManager.SetPossessableInventory(uid.Id, items);
    //    }
    //}
}

