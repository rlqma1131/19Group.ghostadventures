using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryManager : MonoBehaviour
{
   // 기억 수집 및 상태 관리
   public static MemoryManager Instance;

   public List<string> collectedMemoryIDs = new List<string>();
   private Dictionary<string, MemoryData> memoryDataDict = new();

   private List<MemoryData> scannedMemoryList = new();
    public IReadOnlyList<MemoryData> ScannedMemories => scannedMemoryList;

   public event Action<MemoryData> OnMemoryCollected;
   public Button closeMemoryStorage;

   private void Awake()
   {
       if (Instance == null) Instance = this;
       else Destroy(gameObject);
       LoadAllMemoryData();
       closeMemoryStorage.gameObject.SetActive(false);
   }

    private void LoadAllMemoryData()
    {
        var all = Resources.LoadAll<MemoryData>("MemoryData"); // Resources 폴더 사용 시
        foreach (var memory in all)
        {
            if (!memoryDataDict.ContainsKey(memory.memoryTitle))
                memoryDataDict.Add(memory.memoryTitle, memory);
        }
    }

   public void TryCollect(MemoryData memoryData)
   {
        // scan 여부는 MemoryFragment에서 이미 체크한 상태라고 가정

        if (collectedMemoryIDs.Contains(memoryData.memoryTitle)) return;

        collectedMemoryIDs.Add(memoryData.memoryTitle);
        if (!scannedMemoryList.Contains(memoryData))
            scannedMemoryList.Add(memoryData);

        OnMemoryCollected?.Invoke(memoryData);
   }

    public List<MemoryData> GetCollectedMemories()
    {
        List<MemoryData> result = new();
        foreach (var id in collectedMemoryIDs)
        {
            if (memoryDataDict.TryGetValue(id, out var memory))
                result.Add(memory);
        }
        return result;    }

    public bool IsCanStore(MemoryData data) => scannedMemoryList.Contains(data);

    public void OpenMemoryStorage()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(true);
        closeMemoryStorage.gameObject.SetActive(true);
    }
    
    public void CloseMemoryStorage()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(false);
        closeMemoryStorage.gameObject.SetActive(false);
    }

    // ✅ 디버깅용 출력
    public void PrintScannedDebugLog()
    {
        Debug.Log("== [MemoryManager] 스캔된 기억 목록 ==");
        foreach (var memory in scannedMemoryList)
        {
            Debug.Log($"- {memory.memoryID}: {memory.memoryTitle}");
        }
    }

    // ✅ 디버깅용 초기화
    public void ClearScannedDebug()
    {
        scannedMemoryList.Clear();
        collectedMemoryIDs.Clear();
        Debug.Log("[MemoryManager] 스캔 기록 초기화됨");
    }
}
