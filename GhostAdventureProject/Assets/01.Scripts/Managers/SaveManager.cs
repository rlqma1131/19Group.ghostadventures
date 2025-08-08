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
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");
    private static SaveData currentData;

    public static void SaveGame(SaveData data)
    {
        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(SavePath, json);
        Debug.Log($"[SaveManager] 저장됨: {SavePath}");
    }

    public static SaveData LoadGame()
    {
        if (!File.Exists(SavePath))
        {
            Debug.LogWarning("[SaveManager] 저장 파일 없음");
            return null;
        }

        string json = File.ReadAllText(SavePath);
        currentData = JsonUtility.FromJson<SaveData>(json);
        return currentData;
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }

    public static bool IsPuzzleSolved(string puzzleID)
    {
        return currentData?.solvedPuzzleIDs?.Contains(puzzleID) ?? false;
    }

    public static void MarkPuzzleSolved(string puzzleID)
    {
        if (currentData == null)
            currentData = new SaveData();

        if (currentData.solvedPuzzleIDs == null)
            currentData.solvedPuzzleIDs = new List<string>();

        if (!currentData.solvedPuzzleIDs.Contains(puzzleID))
            currentData.solvedPuzzleIDs.Add(puzzleID);
    }
}
