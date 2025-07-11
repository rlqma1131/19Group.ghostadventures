using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MemoryStorage : MonoBehaviour, IUIClosable
{
    [SerializeField] private RectTransform nodeContainer;
    [SerializeField] private GameObject memoryNodePrefab;
    // [SerializeField] private LineRenderer lineRenderer;

    // private List<Transform> nodePositions = new();
    private List<RectTransform> nodeRects = new();
    [SerializeField] private float spacing = 600;
    public Button closeButton;

    //private void OnEnable()
    //{
    //    MemoryManager.Instance.OnMemoryCollected += AddMemoryNode;
    //    RedrawStorage(); // 초기화 시
    //}

    //private void OnDisable()
    //{
    //    MemoryManager.Instance.OnMemoryCollected -= AddMemoryNode;
    //}

    private void RedrawStorage()
    {
        // 저장된 기억을 기준으로 UI 다시 그림
        foreach (Transform child in nodeContainer) Destroy(child.gameObject);
        nodeRects.Clear();

        // 만약 수집된 메모리를 직접 가져올 수 있다면:
        foreach (MemoryData memory in MemoryManager.Instance.GetCollectedMemories())
        {
            AddMemoryNode(memory);
        }
    }

    private void AddMemoryNode(MemoryData memory)
    {
        GameObject node = Instantiate(memoryNodePrefab, nodeContainer);
        node.GetComponent<MemoryNode>().Initialize(memory);
        RectTransform rect = node.GetComponent<RectTransform>();
        if (nodeRects.Count == 0)
        {
            // 첫 번째 노드: 중앙에 배치
            rect.anchoredPosition = Vector2.zero;
        }
        else
        {
            // 이전 노드 기준으로 오른쪽에 배치
            RectTransform prev = nodeRects[^1];
            float x = prev.anchoredPosition.x + spacing;
            rect.anchoredPosition = new Vector2(x, 0f);
        }

        rect.localScale = Vector3.one;
        nodeRects.Add(rect);


        // nodeRects.Add(node.transform);

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

    public void Close()
    {
        UIManager.Instance.MemoryStorageUI.gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public bool IsOpen()
    {
        return UIManager.Instance.MemoryStorageUI.gameObject.activeInHierarchy;
    }
}
