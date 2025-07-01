//using System;
//using System.Collections.Generic;
//using UnityEngine;

//public class MemoryManager : MonoBehaviour
//{
//    // 기억 수집 및 상태 관리
//    public static MemoryManager Instance;

//    public List<string> collectedMemoryIDs = new List<string>();
//    public event Action<MemoryData> OnMemoryCollected;

//    private void Awake()
//    {
//        if (Instance == null) Instance = this;
//        else Destroy(gameObject);
//    }

//    public void TryCollect(MemoryData memoryData)
//    {
//        // scan 여부는 MemoryFragment에서 이미 체크한 상태라고 가정
//        if (collectedMemoryIDs.Contains(memoryData.memoryTitle)) return;

//        collectedMemoryIDs.Add(memoryData.memoryTitle);
//        OnMemoryCollected?.Invoke(memoryData);
//    }

//     public List<MemoryData> GetCollectedMemories()
//     {
//         // 필요 시 UI 초기화용으로 호출
//         return collectedMemoryIDs
//             .Select(id => MemoryData.memoryID/* id로 MemoryData 찾는 로직 필요 */)
//             .ToList();
//     }

    public void OpenMemoryStorage()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(true);
    }
}
