using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using _01.Scripts.Object.BaseClasses.Interfaces;
using Newtonsoft.Json;
using UnityEngine;
using UnityEngine.SceneManagement;
using Object = UnityEngine.Object;

#region Chapter Data Only

/// <summary>
/// 챕터별 기억 기록 상태
/// </summary>
[Serializable] public class ChapterClueProgress
{
    public int chapter;
    public List<string> clueIds = new();
    public bool allCollected;
}

/// <summary>
/// 챕터별 단서 기록 상태
/// </summary> 
[Serializable] public class ChapterMemoryProgress
{
    public int chapter; // 1,2,3
    public List<string> memoryIds = new();
}

#endregion

#region Object Data Only

/// <summary>
/// 기본 Object들의 상태를 저장하기 위한 클래스
/// </summary>
[Serializable] public class ObjectState {
    public string id;
    public bool active;
    public bool isScannable;
    public SerializableVector3 position;

    public ObjectState(string id, bool active = false, bool isScannable = false, Vector3 position = default) {
        this.id = id;
        this.active = active;
        this.isScannable = isScannable;
        this.position = new SerializableVector3(position);
    }
}

/// <summary>
/// 빙의 가능한 Object들의 상태를 저장하기 위한 클래스
/// </summary>
[Serializable] public class PossessableObjectState : ObjectState {
    public bool hasActivated;

    public PossessableObjectState(string id, bool active = false, bool isScannable = false, Vector3 position = default, bool hasActivated = false) :
        base(id, active, isScannable, position) {
        this.hasActivated = hasActivated;
    }
}

/// <summary>
/// 스캔 가능한 기억조각들의 상태를 저장하기 위한 클래스
/// </summary>
[Serializable] public class MemoryFragmentObjectState : ObjectState {
    public MemoryFragmentObjectState(string id, bool active = false, Vector3 position = default, bool isScannable = false) 
        : base(id, active, isScannable, position) { }
}

/// <summary>
/// Animation Clip Layer 상태 저장 클래스
/// </summary>
[Serializable] public class AnimatorClipLayerState
{
    public int layer;
    public string clipName; // 현재 재생 중인 첫 번째 클립 이름
    public int stateHash; // 현재 스테이트(fullPathHash)
    public float normalizedTime;
}

/// <summary>
/// Animation Clip 상태 저장 클래스
/// </summary>
[Serializable] public class AnimatorClipSnapshot
{
    public string id; // UniqueId
    public List<AnimatorClipLayerState> layers = new();
}

/// <summary>
/// 자식 레이어 단 Animation Clip Layer 상태 저장 클래스
/// </summary>
[Serializable] public class AnimatorChildLayerState
{
    public int layer;
    public int fullPathHash;
    public int shortNameHash;
    public float normalizedTime;
    public string clipName; // 디버깅/확인용
}

/// <summary>
/// 자식 레이어 단 Animation Clip들 상태 저장 클래스
/// </summary>
[Serializable] public class AnimatorChildSnapshot
{
    public string childPath;
    public List<AnimatorChildLayerState> layers = new();
}

/// <summary>
/// Animator Tree 상태 저장 클래스 (부모 + 자식)
/// </summary>
[Serializable] public class AnimatorTreeSnapshot
{
    public string id; // UniqueId.Id
    public List<AnimatorChildSnapshot> animators = new();
}

#endregion

#region Tutorial Progress Data Only

// 튜토리얼 진행도 저장용
[Serializable] public class RoomVisitEntry
{
    public string roomName;
    public int count;
}

#endregion

#region Data Transfer Object

[Serializable] public class SaveData
{
    public string sceneName;
    public SerializableVector3 playerPosition;
    public string checkpointId;

    public List<string> collectedClueNames;
    public List<string> collectedMemoryIDs;
    public List<string> scannedMemoryTitles;
    public List<string> solvedPuzzleIDs;

    // 챕터별 진행도 저장
    public List<ChapterMemoryProgress> chapterMemoryProgress = new();
    public List<ChapterClueProgress> chapterClueProgress = new();

    // UniqueId를 갖고 있는 오브젝트 상태 저장 Dictionary
    public Dictionary<string, ObjectState> objectStateDict;
    public Dictionary<string, PossessableObjectState> possessableObjectStateDict;
    public Dictionary<string, MemoryFragmentObjectState> memoryFragmentObjectStateDict;
    public Dictionary<string, bool> doorStateDict;
    public Dictionary<string, bool> eventCompletionDict;

    public Dictionary<string, AnimatorClipSnapshot> animatorClipDict;
    public Dictionary<string, AnimatorTreeSnapshot> animatorTreeDict;

    // 튜토리얼 진행도 저장용
    public List<RoomVisitEntry> roomVisitCounts = new();
    public List<TutorialStep> completedTutorialSteps = new();
}

#endregion

#region Conversion Class of Non-Serializable Values

/// <summary>
/// Vector3를 직렬화 가능하게 만든 SerializableVector3 클래스
/// </summary>
[Serializable] public struct SerializableVector3 : IEquatable<SerializableVector3>
{
    public float x, y, z;

    public SerializableVector3(float x, float y, float z) {
        this.x = x;
        this.y = y;
        this.z = z;
    }

    public SerializableVector3(Vector3 v) {
        x = v.x;
        y = v.y;
        z = v.z;
    }

    public Vector3 ToVector3() => new Vector3(x, y, z);

    #region Operator Methods

    public static bool operator ==(SerializableVector3 v1, SerializableVector3 v2) {
        return Mathf.Approximately(v1.x, v2.x) &&
               Mathf.Approximately(v1.y, v2.y) &&
               Mathf.Approximately(v1.z, v2.z);
    }

    public static bool operator !=(SerializableVector3 v1, SerializableVector3 v2) {
        return !(v1 == v2);
    }

    public static bool operator ==(SerializableVector3 v1, Vector3 v2) {
        return Mathf.Approximately(v1.x, v2.x) &&
               Mathf.Approximately(v1.y, v2.y) &&
               Mathf.Approximately(v1.z, v2.z);
    }

    public static bool operator !=(SerializableVector3 v1, Vector3 v2) {
        return !(v1 == v2);
    }

    #endregion
    
    public bool Equals(SerializableVector3 other) => x.Equals(other.x) && y.Equals(other.y) && z.Equals(other.z);
    public override bool Equals(object obj) => obj is SerializableVector3 other && Equals(other);
    public override int GetHashCode() => HashCode.Combine(x, y, z);
}

#endregion

public static class SaveManager {
    #region Fields

    public static bool VerboseAnimatorLogging = true;
    static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    public static SaveData CurrentData { get; private set; }

    /// <summary>
    /// 세이브 파일 로드 시 발생할 이벤트 Action
    /// </summary>
    public static event Action<SaveData> Loaded;

    #endregion
    
    #region Chapter Progress Saving Methods

    // ===== 챕터 최종단서 수집 진행도 저장/적용 =====
    static ChapterClueProgress GetOrCreateChapterProgress(int chapter) {
        EnsureData();
        var list = CurrentData.chapterClueProgress;
        int idx = list.FindIndex(x => x.chapter == chapter);
        if (idx >= 0) return list[idx];

        var p = new ChapterClueProgress { chapter = chapter };
        list.Add(p);
        return p;
    }

    // Collect될 때마다 호출할 메서드
    public static void AddChapterClue(int chapter, string clueId, bool allCollected = false, bool autosave = false) {
        if (string.IsNullOrEmpty(clueId)) return;
        ChapterClueProgress p = GetOrCreateChapterProgress(chapter);

        if (!p.clueIds.Contains(clueId))
            p.clueIds.Add(clueId);

        // ChapterEndingManager 쪽 판단 결과를 그대로 반영
        if (allCollected) p.allCollected = true;

        if (autosave) SaveGame();
    }

    // 로드시 ChapterEndingManager가 세트로 복구할 때 사용
    public static bool TryGetChapterClueProgress(int chapter, out ChapterClueProgress progress) {
        progress = CurrentData?.chapterClueProgress?.Find(x => x.chapter == chapter);
        return progress != null;
    }

    public static void SetChapterProgress(int chapter, IEnumerable<string> ids, bool allCollected) {
        ChapterClueProgress p = GetOrCreateChapterProgress(chapter);
        p.clueIds = ids != null ? new List<string>(ids) : new List<string>();
        p.allCollected = allCollected;
    }

    // ===== 챕터 기억 수집 진행도 저장/적용 =====
    static ChapterMemoryProgress GetOrCreateChapterMemory(int chapter) {
        EnsureData();
        var list = CurrentData.chapterMemoryProgress;
        int idx = list.FindIndex(x => x.chapter == chapter);
        if (idx >= 0) return list[idx];

        var p = new ChapterMemoryProgress { chapter = chapter };
        list.Add(p);
        return p;
    }

    // 기억 스냅샷 세팅
    public static void SetChapterScannedMemories(int chapter, IEnumerable<string> ids) {
        ChapterMemoryProgress p = GetOrCreateChapterMemory(chapter);
        p.memoryIds = ids != null ? new List<string>(ids) : new List<string>();
    }

    public static bool TryGetChapterScannedMemories(int chapter, out ChapterMemoryProgress progress) {
        progress = CurrentData?.chapterMemoryProgress?.Find(x => x.chapter == chapter);
        return progress != null;
    }

    #endregion
    
    #region Animation Clips & Animator Tree State Saving Methods
    
    // 애니메이터 읽고 쓰기
    public static void SetAnimatorClips(string id, AnimatorClipSnapshot snap) {
        EnsureData();
        
        // Refactored to dictionary
        if (!CurrentData.animatorClipDict.TryAdd(id, snap)) {
            CurrentData.animatorClipDict[id] = snap;
        }
    }
    public static bool TryGetAnimatorClips(string id, out AnimatorClipSnapshot snap) {
        // Refactored to dictionary
        return CurrentData.animatorClipDict.TryGetValue(id, out snap);
    }
    public static void SetAnimatorTree(string id, AnimatorTreeSnapshot snap) {
        EnsureData();

        // Refactored to dictionary
        if (!CurrentData.animatorTreeDict.TryAdd(id, snap)) {
            CurrentData.animatorTreeDict[id] = snap;
        }
    }
    public static bool TryGetAnimatorTree(string id, out AnimatorTreeSnapshot snap) {
        // Refactored to dictionary
        return CurrentData.animatorTreeDict.TryGetValue(id, out snap);
    }

    #endregion
    
    /// <summary>
    /// Save Data 내 Parameter들이 선언되어 있는지 확인 및 선언하는 함수
    /// </summary>
    static void EnsureData() {
        // Initialize save data class or Use existing data
        CurrentData ??= new SaveData();

        // Objects, memory, door, event state dictionary
        CurrentData.objectStateDict ??= new Dictionary<string, ObjectState>();
        CurrentData.possessableObjectStateDict ??= new Dictionary<string, PossessableObjectState>();
        CurrentData.memoryFragmentObjectStateDict ??= new Dictionary<string, MemoryFragmentObjectState>();
        CurrentData.doorStateDict ??= new Dictionary<string, bool>();
        CurrentData.eventCompletionDict ??= new Dictionary<string, bool>();

        // Animator clips and trees dictionary
        CurrentData.animatorClipDict ??= new Dictionary<string, AnimatorClipSnapshot>();
        CurrentData.animatorTreeDict ??= new Dictionary<string, AnimatorTreeSnapshot>();

        // Collected Clues, Memories, Puzzles, Progresses
        CurrentData.collectedClueNames ??= new List<string>();
        CurrentData.collectedMemoryIDs ??= new List<string>();
        CurrentData.scannedMemoryTitles ??= new List<string>();
        CurrentData.solvedPuzzleIDs ??= new List<string>();
        CurrentData.chapterMemoryProgress ??= new List<ChapterMemoryProgress>();
        CurrentData.chapterClueProgress ??= new List<ChapterClueProgress>();
    }

    #region Setter & Getter of data
    
    // ===== 튜토리얼 진행도 저장/적용 =====
    // 방문횟수
    public static void SetRoomVisitCount(string roomName, int count) {
        EnsureData();
        if (CurrentData.roomVisitCounts == null) CurrentData.roomVisitCounts = new List<RoomVisitEntry>();
        int i = CurrentData.roomVisitCounts.FindIndex(x => x.roomName == roomName);
        if (i >= 0) CurrentData.roomVisitCounts[i].count = count;
        else CurrentData.roomVisitCounts.Add(new RoomVisitEntry { roomName = roomName, count = count });
    }
    public static bool TryGetRoomVisitCount(string roomName, out int count) {
        RoomVisitEntry e = CurrentData?.roomVisitCounts?.Find(x => x.roomName == roomName);
        if (e == null) { count = 0; return false;}
        count = e.count;
        return true;
    }

    // 이벤트 완료 목록 상태
    public static void SetEventCompleted(string id, bool completed) {
        EnsureData();
        
        // Refactored to dictionary
        if (!CurrentData.eventCompletionDict.TryAdd(id, completed)) {
            CurrentData.eventCompletionDict[id] = completed;
        }
    }
    public static bool TryGetCompletedEventIds(string id, out bool completed) {
        // Refactored to dictionary
        return CurrentData.eventCompletionDict.TryGetValue(id, out completed);
    }

    // 튜토리얼 완료 목록 상태
    public static void SetCompletedTutorialSteps(IEnumerable<TutorialStep> steps) {
        EnsureData();
        CurrentData.completedTutorialSteps = steps != null
            ? new List<TutorialStep>(steps)
            : new List<TutorialStep>();
    }
    public static bool TryGetCompletedTutorialSteps(out List<TutorialStep> steps) {
        steps = CurrentData?.completedTutorialSteps;
        return steps is { Count: > 0 };
    }
    
    // ===== 일반 ObjectState 저장/적용 =====
    public static void SetObjectState(string id, bool active = false, bool isScannable = false, Vector3 pos = default) {
        EnsureData();
        
        if (CurrentData.objectStateDict.TryAdd(id, new ObjectState(id, active, isScannable, pos))) return;
        CurrentData.objectStateDict[id].active = active;
        CurrentData.objectStateDict[id].isScannable = isScannable;
        CurrentData.objectStateDict[id].position = new SerializableVector3(pos);
    }
    public static bool TryGetObjectState(string id, out ObjectState state) {
        return CurrentData.objectStateDict.TryGetValue(id, out state);
    }
    
    // ===== 빙의 가능한 PossessableObjectState 저장/적용 =====
    public static void SetPossessableObjectState(string id, bool active = false, bool isScannable = false, Vector3 pos = default,
        bool hasActivated = false) {
        EnsureData();
        
        if (CurrentData.possessableObjectStateDict.TryAdd(id, new PossessableObjectState(id, active, isScannable, pos, hasActivated))) return;
        CurrentData.possessableObjectStateDict[id].active = active;
        CurrentData.possessableObjectStateDict[id].isScannable = isScannable;
        CurrentData.possessableObjectStateDict[id].position = new SerializableVector3(pos);
        CurrentData.possessableObjectStateDict[id].hasActivated = hasActivated;
    }
    public static bool TryGetPossessableObjectState(string id, out PossessableObjectState state) {
        return CurrentData.possessableObjectStateDict.TryGetValue(id, out state);
    }

    // ===== 기억조각 MemoryFragmentObjectState 저장/찾기 =====
    public static void SetMemoryFragmentObjectState(string id, bool active = false, Vector3 pos = default,
        bool isScannable = false) {
        EnsureData();

        if (CurrentData.memoryFragmentObjectStateDict.TryAdd(id, new MemoryFragmentObjectState(id, active, pos, isScannable))) return;
        CurrentData.memoryFragmentObjectStateDict[id].active = active;
        CurrentData.memoryFragmentObjectStateDict[id].position = new SerializableVector3(pos);
        CurrentData.memoryFragmentObjectStateDict[id].isScannable = isScannable;
    }
    public static bool TryGetMemoryFragmentObjectState(string id, out MemoryFragmentObjectState state) {
        return CurrentData.memoryFragmentObjectStateDict.TryGetValue(id, out state);
    }
    
    // ===== MemoryFragment 상태 저장/적용 =====
    public static void SetMemoryFragmentScannable(string id, bool isScannable) {
        EnsureData();

        // Refactored to dictionary
        if (CurrentData.memoryFragmentObjectStateDict.TryGetValue(id, out MemoryFragmentObjectState memoryFragment)) {
            memoryFragment.isScannable = isScannable;
        }
    }
    public static bool TryGetMemoryFragmentScannable(string id, out bool isScannable) {
        // Refactored to dictionary
        if (!CurrentData.memoryFragmentObjectStateDict.TryGetValue(id, out MemoryFragmentObjectState state)) {
            isScannable = false;
            return false;
        }
        
        isScannable = state.isScannable;
        return true;
    }

    // ===== 문 상태 저장/적용 =====
    public static void SetDoorLocked(string id, bool isLocked) {
        EnsureData();
        
        // Refactored to dictionary
        if (!CurrentData.doorStateDict.TryAdd(id, isLocked)) {
            CurrentData.doorStateDict[id] = isLocked;
        }
    }
    public static bool TryGetDoorLocked(string id, out bool isLocked) {
        // Refactored to dictionary
        return CurrentData.doorStateDict.TryGetValue(id, out isLocked);
    }

    #endregion

    /// <summary>
    /// UniqueID를 갖고 있는 오브젝트 활성화 상태 스냅샷
    /// </summary>
    static void SnapshotAllUniqueIdObjects() {
        // true: 비활성 포함, 모든 로드된 씬 대상
        var uids = Object.FindObjectsOfType<UniqueId>(true);

        foreach (UniqueId uid in uids) {
            GameObject go = uid.gameObject;

            // DontDestroyOnLoad에 있는 전역 오브젝트는 스킵 (옵션)
            Scene scn = go.scene;
            if (!scn.IsValid() || !scn.isLoaded || scn.name == "DontDestroyOnLoad")
                continue;

            Debug.Log($"[SaveManager] 오브젝트 : {uid.Id}, 활성화 상태 : {go.activeInHierarchy}");

            // Refactored Saving Method
            if (go.TryGetComponent(out BasePossessable possessable))
                SetPossessableObjectState(uid.Id, go.activeInHierarchy, possessable.IsScannable(), go.transform.position, possessable.HasActivated());
            else if (go.TryGetComponent(out MemoryFragment memoryFragment))
                SetMemoryFragmentObjectState(uid.Id, go.activeInHierarchy, go.transform.position, memoryFragment.IsScannable());
            else if (go.TryGetComponent(out BaseDoor door))
                SetDoorLocked(uid.Id, door.IsLocked);
            else if (go.TryGetComponent(out Ch2_DrawingClue clue))
                SetPossessableObjectState(uid.Id, clue.gameObject.activeInHierarchy, clue.IsScannable(), clue.transform.position, clue.HasActivated);
            else if (go.TryGetComponent(out IInteractable interactable))
                SetObjectState(uid.Id, go.activeInHierarchy, interactable.IsScannable(), go.transform.position);
            else 
                SetObjectState(uid.Id, go.activeInHierarchy, pos: go.transform.position);
        }
    }

    /// <summary>
    /// Animator Tree 저장 메소드
    /// </summary>
    public static void SnapshotAllAnimatorTrees() {
        var uids = GameObject.FindObjectsOfType<UniqueId>(true);

        foreach (UniqueId uid in uids) {
            Transform root = uid.transform;
            var anims = root.GetComponentsInChildren<Animator>(true);
            if (anims == null || anims.Length == 0) continue;

            var tree = new AnimatorTreeSnapshot { id = uid.Id };
            var sb = new StringBuilder();
            sb.Append($"[SNAP-TREE] uid={uid.Id} go='{uid.gameObject.name}' animators={anims.Length}");

            foreach (Animator anim in anims) {
                string childPath = GetRelativePath(root, anim.transform);
                if (string.IsNullOrEmpty(childPath)) continue;

                var childSnap = new AnimatorChildSnapshot { childPath = childPath };

                int lc = anim.layerCount;
                for (int l = 0; l < lc; l++) {
                    AnimatorStateInfo info = anim.GetCurrentAnimatorStateInfo(l);

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
                foreach (AnimatorChildLayerState ly in childSnap.layers) {
                    sb.Append(
                        $" | L{ly.layer}: full={ly.fullPathHash} short={ly.shortNameHash} norm={ly.normalizedTime:F3} clip='{ly.clipName}'");
                }
            }

            SetAnimatorTree(uid.Id, tree);
        }
    }

    // ===== 플레이어 인벤토리 상태 적용 =====
    public static void ApplyPlayerInventoryFromSave(Inventory_Player inv) {
        if (CurrentData?.collectedClueNames == null) return;

        inv.RemoveClueBeforeStage(); // 전체 초기화
        foreach (string clueName in CurrentData.collectedClueNames) {
            // 저장한 기준에 맞춰 로드 경로/키 선택
            ClueData clue = Resources.Load<ClueData>("ClueData/" + clueName); // (에셋 파일명 기준)
            // var clue = /* clue_Name 기준이면 */ Resources.LoadAll<ClueData>("ClueData")
            //               .FirstOrDefault(c => c.clue_Name == clueName);

            if (clue != null) inv.AddClue(clue);
        }
    }
    
    public static string GetRelativePath(Transform root, Transform target) {
        if (!root || !target) return null;
        var stack = new Stack<string>();
        Transform t = target;
        while (t && t != root) {
            stack.Push(t.name);
            t = t.parent;
        }

        if (t != root) return null; // target이 root 하위가 아님
        var sb = new StringBuilder();
        while (stack.Count > 0) {
            if (sb.Length > 0) sb.Append('/');
            sb.Append(stack.Pop());
        }

        return sb.ToString();
    }

    public static Transform FindChildByPath(Transform root, string path) {
        if (root == null || string.IsNullOrEmpty(path)) return null;
        // Transform.Find("A/B/C")는 비활성 자식도 찾음
        return root.Find(path);
    }

    // 저장된 AnimatorClipSnapshot들을 보기 좋은 형태로 찍어주는 유틸
    public static void DumpAnimatorClipSnapshots(string header = "[Dump]", int maxObjects = 20, int maxLayers = 4) {
        if (CurrentData == null) return;
        
        int count = CurrentData.animatorClipDict?.Count ?? 0;
        if (count == 0) return;
        
        int printed = 0;
        foreach (AnimatorClipSnapshot snap in CurrentData.animatorClipDict.Values) {
            if (snap == null) continue;
            int lc = Mathf.Min(snap.layers?.Count ?? 0, maxLayers);
            for (int i = 0; i < lc; i++) {
                AnimatorClipLayerState l = snap.layers[i];
            }

            printed++;
        }
    }
    
    // ===== 조회(읽기) 계열 =====
    public static bool HasSaveFile() => File.Exists(SavePath);
    public static bool IsPuzzleSolved(string puzzleID) => CurrentData?.solvedPuzzleIDs?.Contains(puzzleID) ?? false;
    public static bool HasCollectedClue(string clueName) => CurrentData?.collectedClueNames?.Contains(clueName) ?? false;
    public static bool HasCollectedMemoryID(string memoryID) => CurrentData?.collectedMemoryIDs?.Contains(memoryID) ?? false;
    public static string GetLastSceneName() => CurrentData?.sceneName ?? string.Empty;
    public static Vector3? GetLastPlayerPosition() => CurrentData?.playerPosition.ToVector3();
    public static string GetCheckpointId() => CurrentData?.checkpointId ?? string.Empty;

    /// <summary>
    /// 모든 데이터를 저장하는 메소드
    /// </summary>
    /// <param name="data">모든 Object 정보들이 저장된 DTO class</param>
    public static void SaveGame(SaveData data = null) {
        if (data != null) CurrentData = data;
        EnsureData();

        try {
            string json = JsonConvert.SerializeObject(CurrentData, Formatting.Indented);
            string tmp = SavePath + ".tmp";
            File.WriteAllText(tmp, json);
            if (File.Exists(SavePath)) File.Delete(SavePath);
            File.Move(tmp, SavePath);

            int animClipCount = CurrentData.animatorClipDict?.Count ?? 0;

            Debug.Log($"[SaveManager] Saved to: {SavePath}\n" +
                      $" scene={CurrentData.sceneName}" +
                      $" memIDs={CurrentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={CurrentData.collectedClueNames?.Count ?? 0}" +
                      $" animClips={animClipCount}" +
                      $" bytes={json.Length}");
        }
        catch (Exception ex) {
            Debug.LogError($"[SaveManager] Save FAILED at {SavePath}\n{ex}");
        }

        NoticePopup popup = UIManager.Instance != null ? UIManager.Instance.SaveNoticePopupUI : null;
        if (popup != null) popup.FadeInAndOut("게임이 저장되었습니다.");
    }
    
    /// <summary>
    /// 내부 저장소에 저장된 데이터들을 부르는 메소드
    /// </summary>
    /// <returns></returns>
    public static SaveData LoadGame() {
        if (!File.Exists(SavePath)) {
            CurrentData = null;
            return null;
        }

        try {
            string json = File.ReadAllText(SavePath);
            CurrentData = JsonConvert.DeserializeObject<SaveData>(json);
            EnsureData();

            int animClipCount = CurrentData.animatorClipDict?.Count ?? 0;

            Debug.Log($"[SaveManager] Loaded. scene={CurrentData.sceneName}" +
                      $" memIDs={CurrentData.collectedMemoryIDs?.Count ?? 0}" +
                      $" clues={CurrentData.collectedClueNames?.Count ?? 0}" +
                      $" animClips={animClipCount}" + // <-- 추가
                      $" bytes={json.Length}");
        }
        catch (Exception) {
            CurrentData = new SaveData();
            EnsureData();
        }

        // 로드된 내용 요약 덤프
        DumpAnimatorClipSnapshots("[LOAD] snapshot summary", 10, 2);

        Loaded?.Invoke(CurrentData);
        return CurrentData;
    }

    /// <summary>
    /// 저장된 데이터 파일을 삭제하는 메소드
    /// </summary>
    public static void DeleteSave() {
        if (File.Exists(SavePath)) File.Delete(SavePath);
        CurrentData = null; // 조회 계열은 이후 false/기본값을 반환
    }

    /// <summary>
    /// 퍼즐 풀기를 완료 시 불리는 메소드
    /// </summary>
    /// <param name="puzzleID"></param>
    public static void MarkPuzzleSolved(string puzzleID) {
        EnsureData();
        if (!CurrentData.solvedPuzzleIDs.Contains(puzzleID)) {
            CurrentData.solvedPuzzleIDs.Add(puzzleID);
            // Choose to save or not -> if (autosave) SaveGame();
        }
    }

    /// <summary>
    /// 단서 수집 시 불리는 메소드
    /// </summary>
    /// <param name="clueName"></param>
    public static void AddCollectedClue(string clueName) {
        EnsureData();
        if (!CurrentData.collectedClueNames.Contains(clueName)) {
            CurrentData.collectedClueNames.Add(clueName);
            // Choose to save or not -> if (autosave) SaveGame();
        }
    }

    /// <summary>
    /// 기억 조각을 스캔 시 불리는 메소드
    /// </summary>
    /// <param name="memoryID"></param>
    public static void AddCollectedMemoryID(string memoryID) {
        EnsureData();
        if (!CurrentData.collectedMemoryIDs.Contains(memoryID)) {
            CurrentData.collectedMemoryIDs.Add(memoryID);
            // Choose to save or not -> if (autosave) SaveGame();
        }
    }

    // 기억 스캔했을 때 기억제목 저장
    public static void AddScannedMemoryTitle(string title) {
        EnsureData();
        if (!CurrentData.scannedMemoryTitles.Contains(title)) {
            CurrentData.scannedMemoryTitles.Add(title);
            //if (autosave) SaveGame();
        }
    }

    // 플레이어 인벤토리 상태 저장
    public static void SnapshotPlayerInventory(Inventory_Player inv) {
        EnsureData();
        CurrentData.collectedClueNames.Clear();

        // 에셋 파일명으로 저장
        foreach (ClueData c in inv.collectedClues) {
            if (c != null) CurrentData.collectedClueNames.Add(c.name);
        }
    }

    // 현재 씬 이름과 위치 저장
    public static void SetSceneAndPosition(string sceneName, Vector3 playerPos, string checkpointId = null,
        bool autosave = true) {
        EnsureData();
        
        CurrentData.sceneName = sceneName;
        CurrentData.playerPosition = new SerializableVector3(playerPos);
        CurrentData.checkpointId = checkpointId;
    }

    // 기억 스캔 ScanedCheck 할 때 호출
    public static void SaveWhenScanAfter(string memoryID, string title,
        string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true) {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);
        SnapshotAllUniqueIdObjects();

        // 플레이어 인벤토리 상태 저장
        Inventory_Player inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SnapshotPlayerInventory(inv);

        // 빙의 대상 인벤토리 상태 저장
        //SnapshotAllPossessableInventories();

        ChapterEndingManager cem = ChapterEndingManager.Instance;
        if (cem != null) {
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
        DumpAnimatorClipSnapshots("[SAVE] snapshot summary", 10, 2);

        if (autosave) SaveGame();
    }

    //// 이벤트 재생 후 저장
    //public static void SaveWhenAfterEvent(
    //    string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    //{
    //    SnapshotAllUniqueIdActivesAndPositions();

    //    // 오브젝트 SetAcitve 상태, 위치 저장
    //    foreach (var p in GameObject.FindObjectsOfType<BasePossessable>(true))
    //    {
    //        if (p.TryGetComponent(out UniqueId uid))
    //        {
    //            SetActiveState(uid.Id, p.gameObject.activeSelf);
    //            SetObjectPosition(uid.Id, p.transform.position);

    //            SetPossessableState(uid.Id, p.HasActivated);
    //        }
    //    }

    //    foreach (var m in GameObject.FindObjectsOfType<MemoryFragment>(true))
    //    {
    //        if (m.TryGetComponent(out UniqueId uid))
    //        {
    //            SetActiveState(uid.Id, m.gameObject.activeSelf);
    //            SetObjectPosition(uid.Id, m.transform.position);

    //            SetMemoryFragmentScannable(uid.Id, m.IsScannable);
    //        }
    //    }

    //    // 문 상태 저장
    //    foreach (var door in GameObject.FindObjectsOfType<BaseDoor>(true))
    //    {
    //        if (door.TryGetComponent(out UniqueId uid))
    //            SaveManager.SetDoorLocked(uid.Id, door.IsLocked);
    //    }

    //    // 플레이어 인벤토리 상태 저장
    //    var inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
    //    SnapshotPlayerInventory(inv);

    //    var cem = ChapterEndingManager.Instance;
    //    if (cem != null)
    //    {
    //        Debug.Log($"[SAVE] ch1 clues now={cem.CurrentCh1ClueCount}");

    //        // 최종단서 진행도 스냅샷
    //        SetChapterProgress(1, cem.GetClueIds(1), cem.GetAllCollected(1));
    //        SetChapterProgress(2, cem.GetClueIds(2), cem.GetAllCollected(2));

    //        // 스캔된 기억 스냅샷
    //        SetChapterScannedMemories(1, cem.GetScannedMemoryIds(1));
    //        SetChapterScannedMemories(2, cem.GetScannedMemoryIds(2));
    //        SetChapterScannedMemories(3, cem.GetScannedMemoryIds(3));
    //    }

    //    SetSceneAndPosition(sceneName, playerPos, checkpointId, autosave);

    //    // 애니메이터 상태 스탭샷
    //    SnapshotAllAnimatorTrees();
    //    DumpAnimatorClipSnapshots("[SAVE] snapshot summary", maxObjects: 10, maxLayers: 2);

    //    if (autosave) SaveGame();
    //}

    // 기억조각 컷씬 자동재생 후 저장
    public static void SaveWhenCutScene(string memoryID, string title,
        string sceneName, string checkpointId = null, bool autosave = true) {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);
        SnapshotAllUniqueIdObjects();

        // 플레이어 인벤토리 상태 저장
        Inventory_Player inv = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        SnapshotPlayerInventory(inv);

        ChapterEndingManager cem = ChapterEndingManager.Instance;
        if (cem != null) {
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
        DumpAnimatorClipSnapshots("[SAVE] snapshot summary", 10, 2);

        if (autosave) SaveGame();
    }
}