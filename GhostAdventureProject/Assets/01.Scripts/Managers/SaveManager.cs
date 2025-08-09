using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public string checkpointId;

    public List<string> collectedClueNames;      // clue_Name (아이템 이름)
    public List<string> collectedMemoryIDs;      // memoryID (MemoryData의 고유값)
    public List<string> scannedMemoryTitles;     // memoryTitle

    public List<string> solvedPuzzleIDs;
    // 인벤토리, 능력치 등 추가
}

public static class SaveManager
{
    // ===== 공통 =====
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentData;
    public static SaveData Current => currentData; // 읽기 전용 참조(주의: 외부에서 수정하지 말 것)

    private static void EnsureData()
    {
        if (currentData == null) currentData = new SaveData();
        if (currentData.collectedClueNames == null) currentData.collectedClueNames = new List<string>();
        if (currentData.collectedMemoryIDs == null) currentData.collectedMemoryIDs = new List<string>();
        if (currentData.scannedMemoryTitles == null) currentData.scannedMemoryTitles = new List<string>();
        if (currentData.solvedPuzzleIDs == null) currentData.solvedPuzzleIDs = new List<string>();
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
