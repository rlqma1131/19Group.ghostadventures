using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryStorage : MonoBehaviour
{
    [SerializeField] private Transform nodeContainer;
    [SerializeField] private GameObject memoryNodePrefab;
    // [SerializeField] private LineRenderer lineRenderer;

    private List<Transform> nodePositions = new();

    private void OnEnable()
    {
        MemoryManager.Instance.OnMemoryCollected += AddMemoryNode;
        RedrawStorage(); // 초기화 시
    }

    private void OnDisable()
    {
        MemoryManager.Instance.OnMemoryCollected -= AddMemoryNode;
    }

    private void RedrawStorage()
    {
        // 저장된 기억을 기준으로 UI 다시 그림
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        nodePositions.Clear();

        // 만약 수집된 메모리를 직접 가져올 수 있다면:
        // foreach (MemoryData memory in MemoryManager.Instance.GetCollectedMemories())
        // {
        //     AddMemoryNode(memory);
        // }
    }

    private void AddMemoryNode(MemoryData memory)
    {
        GameObject node = Instantiate(memoryNodePrefab, nodeContainer);
        node.GetComponent<MemoryNode>().Initialize(memory);
        nodePositions.Add(node.transform);

        UpdateLine();
    }

    private void UpdateLine()
    {
        // lineRenderer.positionCount = nodePositions.Count;
        // for (int i = 0; i < nodePositions.Count; i++)
        // {
        //     lineRenderer.SetPosition(i, nodePositions[i].position);
        // }
    }
}
