using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

// 오브젝트 SetActive 상태
[System.Serializable]
public class ActiveObjectState
{
    public string id;
    public bool active;
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

    public List<ActiveObjectState> activeObjectStates;
    public List<DoorState> doorStates;
    public List<PossessableState> possessableStates;
    public List<MemoryFragmentState> memoryFragmentStates;
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
        if (currentData.collectedClueNames == null) currentData.collectedClueNames = new List<string>();
        if (currentData.collectedMemoryIDs == null) currentData.collectedMemoryIDs = new List<string>();
        if (currentData.scannedMemoryTitles == null) currentData.scannedMemoryTitles = new List<string>();
        if (currentData.solvedPuzzleIDs == null) currentData.solvedPuzzleIDs = new List<string>();
        if (currentData.activeObjectStates == null) currentData.activeObjectStates = new List<ActiveObjectState>();
        if (currentData.doorStates == null) currentData.doorStates = new List<DoorState>();
        if (currentData.possessableStates == null) currentData.possessableStates = new List<PossessableState>();               // ★
        if (currentData.memoryFragmentStates == null) currentData.memoryFragmentStates = new List<MemoryFragmentState>();       // ★
    }

    // ===== 오브젝트 활성화 상태 저장/조회 =====
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

    // ===== BasePossessable 상태 저장/조회 =====
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


    // ===== MemoryFragment 상태 저장/조회 =====
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

    // ===== 문 상태 저장/조회 =====
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

        string json = JsonUtility.ToJson(currentData, true);
        string tmp = SavePath + ".tmp";
        File.WriteAllText(tmp, json);
        if (File.Exists(SavePath)) File.Delete(SavePath);
        File.Move(tmp, SavePath);

        UIManager.Instance.SaveNoticePopupUI.FadeInAndOut("게임이 저장되었습니다.");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            currentData = null;
            return null;
        }

        string json = File.ReadAllText(SavePath);
        currentData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        EnsureData();

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

    // 현재 씬 이름과 위치 저장
    public static void SetSceneAndPosition(string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    {
        EnsureData();
        currentData.sceneName = sceneName;
        currentData.playerPosition = playerPos;
        currentData.checkpointId = checkpointId;
    }

    // 기억 스캔할 때 호출 ( 기억ID, 기억제목, 플레이어 위치 저장 )
    public static void SaveWhenScan(string memoryID, string title,
    string sceneName, Vector3 playerPos, string checkpointId = null, bool autosave = true)
    {
        AddCollectedMemoryID(memoryID);
        AddScannedMemoryTitle(title);

        // 오브젝트 SetAcitve 상태 저장
        foreach (var p in GameObject.FindObjectsOfType<BasePossessable>(true))
        {
            if (p.TryGetComponent(out UniqueId uid))
                SetActiveState(uid.Id, p.gameObject.activeSelf);
        }
        foreach (var m in GameObject.FindObjectsOfType<MemoryFragment>(true))
        {
            if (m.TryGetComponent(out UniqueId uid))
                SetActiveState(uid.Id, m.gameObject.activeSelf);
        }

        SetSceneAndPosition(sceneName, playerPos, checkpointId, autosave);
        if (autosave) SaveGame();
    }
}
