using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryStorage : MonoBehaviour
{
    [SerializeField] private Transform nodeContainer; // 노드를 담을 부모 오브젝트
    [SerializeField] private GameObject memoryNodePrefab;

    private void OnEnable()
    {
        MemoryManager.Instance.OnMemoryCollected += AddMemoryNode;
    }

    private void OnDisable()
    {
        MemoryManager.Instance.OnMemoryCollected -= AddMemoryNode;
    }

    private void AddMemoryNode(MemoryData memory)
    {
        GameObject node = Instantiate(memoryNodePrefab, nodeContainer);
        MemoryNode nodeScript = node.GetComponent<MemoryNode>();
        nodeScript.Initialize(memory);
    }}
