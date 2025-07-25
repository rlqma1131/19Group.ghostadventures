using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

[System.Serializable]
public class SaveDatas
{
    public List<string> solvedPuzzleIDs = new List<string>();
}

public class PuzzleStateManager : MonoBehaviour
{
    public static PuzzleStateManager Instance { get; private set; }

    private HashSet<string> solvedPuzzles = new HashSet<string>();

    private string savePath => Application.persistentDataPath + "/save.json";


    private void Awake() 
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }         
    }

    public void MarkPuzzleSolved(string puzzleID)
    {
        solvedPuzzles.Add(puzzleID);
    }

    public bool IsPuzzleSolved(string puzzleID)
    {
        return solvedPuzzles.Contains(puzzleID);
    }

    public void Save()
    {
        SaveDatas data = new SaveDatas();
        data.solvedPuzzleIDs = new List<string>(solvedPuzzles);

        string json = JsonUtility.ToJson(data, true);
        File.WriteAllText(savePath, json);

        Debug.Log("✅ 퍼즐 상태 저장됨");
    }

    public void Load()
    {
        if (File.Exists(savePath))
        {
            string json = File.ReadAllText(savePath);
            SaveDatas data = JsonUtility.FromJson<SaveDatas>(json);

            solvedPuzzles = new HashSet<string>(data.solvedPuzzleIDs);

            Debug.Log("✅ 퍼즐 상태 불러옴");
        }
        else
        {
            Debug.Log("⚠️ 저장 파일 없음");
        }
    }
}

