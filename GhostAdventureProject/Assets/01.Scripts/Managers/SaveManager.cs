using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveData
{
    public string sceneName;
    public Vector3 playerPosition;
    public string checkpointId;

    // 인벤토리, 능력치 등 추가
}

public static class SaveManager
{
    private static string SavePath => Path.Combine(Application.persistentDataPath, "save.json");

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
        return JsonUtility.FromJson<SaveData>(json);
    }

    public static void DeleteSave()
    {
        if (File.Exists(SavePath))
            File.Delete(SavePath);
    }
}
