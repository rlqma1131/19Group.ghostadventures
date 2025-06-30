using System;
using System.Collections.Generic;
using UnityEngine;

public class MemoryManager : MonoBehaviour
{
    // 기억 수집 및 상태 관리
    public static MemoryManager Instance;

    private HashSet<string> collectedMemoryIDs = new HashSet<string>();

    public event Action<MemoryData> OnMemoryCollected;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void CollectMemory(MemoryData memoryData)
    {
        if (collectedMemoryIDs.Contains(memoryData.memoryID)) return;

        collectedMemoryIDs.Add(memoryData.memoryID);
        OnMemoryCollected?.Invoke(memoryData); // UI 업데이트 트리거
    }

    public bool HasCollected(string memoryID) => collectedMemoryIDs.Contains(memoryID); 
}
