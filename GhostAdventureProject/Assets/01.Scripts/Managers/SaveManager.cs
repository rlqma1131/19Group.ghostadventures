using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class PossessableState
{
    public string id;
    public bool hasActivated;
}

[System.Serializable]
public class MemoryFragmentState
{
    public string id;
    public bool isScannable;
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

    public List<PossessableState> possessableStates;
    public List<MemoryFragmentState> memoryFragmentStates;
}

public static class SaveManager
{
    // ===== 공통 =====
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentData;
    public static SaveData CurrentData => currentData;

    private static void EnsureData()
    {
        if (currentData == null) currentData = new SaveData();
        if (currentData.collectedClueNames == null) currentData.collectedClueNames = new List<string>();
        if (currentData.collectedMemoryIDs == null) currentData.collectedMemoryIDs = new List<string>();
        if (currentData.scannedMemoryTitles == null) currentData.scannedMemoryTitles = new List<string>();
        if (currentData.solvedPuzzleIDs == null) currentData.solvedPuzzleIDs = new List<string>();
        if (currentData.possessableStates == null) currentData.possessableStates = new List<PossessableState>();               // ★
        if (currentData.memoryFragmentStates == null) currentData.memoryFragmentStates = new List<MemoryFragmentState>();       // ★
    }

    // ===== BasePossessable 상태 저장/조회 =====
    public static void SetPossessableState(string id, bool hasActivated)
    {
        EnsureData();
        var list = currentData.possessableStates;
        int idx = list.FindIndex(x => x.id == id);
        if (idx >= 0) list[idx].hasActivated = hasActivated;
        else list.Add(new PossessableState { id = id, hasActivated = hasActivated });
    }

    public static bool TryGetPossessableState(string id, out bool hasActivated)
    {
        hasActivated = true; // 기본값(디폴트로 활성)
        if (currentData == null || currentData.possessableStates == null) return false;
        var found = currentData.possessableStates.Find(x => x.id == id);
        if (found == null) return false;
        hasActivated = found.hasActivated;
        return true;
    }


    // ===== MemoryFragment 상태 저장/조회 =====
    public static void SetMemoryFragmentState(string id, bool isScannable, bool canStore)
    {
        EnsureData();
        var list = currentData.memoryFragmentStates;
        int idx = list.FindIndex(x => x.id == id);
        if (idx >= 0)
        {
            list[idx].isScannable = isScannable;
        }
        else
        {
            list.Add(new MemoryFragmentState { id = id, isScannable = isScannable });
        }
    }

    public static bool TryGetMemoryFragmentState(string id, out bool isScannable, out bool canStore)
    {
        isScannable = false; canStore = false;
        if (currentData == null || currentData.memoryFragmentStates == null) return false;
        var found = currentData.memoryFragmentStates.Find(x => x.id == id);
        if (found == null) return false;
        isScannable = found.isScannable;
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
            Debug.LogWarning("[SaveManager] 저장 파일 없음");
            currentData = null; // 조회 계열은 false/기본값 반환
            return null;
        }

        string json = File.ReadAllText(SavePath);
        currentData = JsonUtility.FromJson<SaveData>(json) ?? new SaveData();
        EnsureData(); // 로드 직후 내부 구조 보정(쓰기 관점)
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
        SetSceneAndPosition(sceneName, playerPos, checkpointId, autosave);
        if (autosave) SaveGame();
    }
}
