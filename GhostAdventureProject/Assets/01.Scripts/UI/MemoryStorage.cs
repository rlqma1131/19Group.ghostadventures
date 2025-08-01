using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

// public class MemoryStorage : MonoBehaviour, IUIClosable
// {
//     [SerializeField] private RectTransform nodeContainer;
//     [SerializeField] private GameObject memoryNodePrefab;
//     // [SerializeField] private LineRenderer lineRenderer;

//     // private List<Transform> nodePositions = new();
//     private List<RectTransform> nodeRects = new();
//     [SerializeField] private float spacing = 600;
//     public Button closeButton;

//     private void OnEnable()
//     {
//        MemoryManager.Instance.OnMemoryCollected += AddMemoryNode;
//        RedrawStorage(); // 초기화 시
//     }

//     private void OnDisable()
//     {
//        MemoryManager.Instance.OnMemoryCollected -= AddMemoryNode;
//     }

//     private void RedrawStorage()
//     {
//         // 저장된 기억을 기준으로 UI 다시 그림
//         foreach (Transform child in nodeContainer) Destroy(child.gameObject);
//         nodeRects.Clear();

//         // 만약 수집된 메모리를 직접 가져올 수 있다면:
//         foreach (MemoryData memory in MemoryManager.Instance.GetCollectedMemories())
//         {
//             AddMemoryNode(memory);
//         }
//     }

//     private void AddMemoryNode(MemoryData memory)
//     {
//         GameObject node = Instantiate(memoryNodePrefab, nodeContainer);
//         node.GetComponent<MemoryNode>().Initialize(memory);
//         RectTransform rect = node.GetComponent<RectTransform>();
//         if (nodeRects.Count == 0)
//         {
//             // 첫 번째 노드: 중앙에 배치
//             rect.anchoredPosition = Vector2.zero;
//         }
//         else
//         {
//             // 이전 노드 기준으로 오른쪽에 배치
//             RectTransform prev = nodeRects[^1];
//             float x = prev.anchoredPosition.x + spacing;
//             rect.anchoredPosition = new Vector2(x, 0f);
//         }

//         rect.localScale = Vector3.one;
//         nodeRects.Add(rect);


//         // nodeRects.Add(node.transform);

//         UpdateLine();
//     }

//     private void UpdateLine()
//     {
//         // lineRenderer.positionCount = nodePositions.Count;
//         // for (int i = 0; i < nodePositions.Count; i++)
//         // {
//         //     lineRenderer.SetPosition(i, nodePositions[i].position);
//         // }
//     }

//     public void Close()
//     {
//         UIManager.Instance.MemoryStorageUI.gameObject.SetActive(false);
//         closeButton.gameObject.SetActive(false);
//     }

//     public bool IsOpen()
//     {
//         return UIManager.Instance.MemoryStorageUI.gameObject.activeInHierarchy;
//     }
// }

public class MemoryStorage : MonoBehaviour, IUIClosable
{
    [SerializeField] private GameObject memoryNodePrefab;
    [SerializeField] private Transform leftPageSlot;
    [SerializeField] private Transform rightPageSlot;
    [SerializeField] private Sprite defaultPageSprite; // 기본 페이지 이미지
    [SerializeField] private Image pageTurnImage; // 페이지 넘김 효과 표시할 이미지
    [SerializeField] private Sprite[] pageTurnSprites; // 프레임 순서대로 넣기
    [SerializeField] private float frameInterval = 0.05f; // 프레임 간 간격

    private bool isFlipping = false;

    [SerializeField] private Button nextPageButton;
    [SerializeField] private Button prevPageButton;
    [SerializeField] private Button closeButton;

    private List<MemoryData> collectedMemories = new();
    public List<MemoryData> CollectedMemories => collectedMemories;

    private int currentPageIndex = 0; // 0: 첫 페이지, 1: 다음 페이지...

    private void OnEnable()
    {
        MemoryManager.Instance.OnMemoryCollected += OnMemoryCollected;
        RedrawStorage();
    }

    private void OnDisable()
    {
        MemoryManager.Instance.OnMemoryCollected -= OnMemoryCollected;
    }

    private void OnMemoryCollected(MemoryData memory)
    {
        collectedMemories.Add(memory);
        ShowPage(currentPageIndex); // 현재 페이지 갱신
    }

    private void RedrawStorage()
    {
        collectedMemories = MemoryManager.Instance.GetCollectedMemories();
        currentPageIndex = 0;
        ShowPage(currentPageIndex);
    }

    private void ShowPage(int pageIndex)
    {
        ClearPage();

        int leftIndex = pageIndex * 2;
        int rightIndex = leftIndex + 1;

        if (leftIndex < collectedMemories.Count)
            CreateMemoryNode(leftPageSlot, collectedMemories[leftIndex]);

        if (rightIndex < collectedMemories.Count)
            CreateMemoryNode(rightPageSlot, collectedMemories[rightIndex]);

        // 버튼 활성화 여부
        prevPageButton.interactable = pageIndex > 0;
        nextPageButton.interactable = (pageIndex + 1) * 2 < collectedMemories.Count;
    }

    private void CreateMemoryNode(Transform parent, MemoryData memory)
    {
        GameObject node = Instantiate(memoryNodePrefab, parent);
        node.GetComponent<MemoryNode>().Initialize(memory);
    }

    private void ClearPage()
    {
        foreach (Transform child in leftPageSlot) Destroy(child.gameObject);
        foreach (Transform child in rightPageSlot) Destroy(child.gameObject);
    }

    public void OnClickNextPage()
    {
        // TODO: 책장 넘김 애니메이션
        if (isFlipping || currentPageIndex + 1 >= pageTurnSprites.Length) return;

        currentPageIndex++;
        PlayPageTurnAnimation(() => ShowPage(currentPageIndex));
    }

    public void OnClickPrevPage()
    {
        currentPageIndex--;
        ShowPage(currentPageIndex);
        // TODO: 책장 넘김 애니메이션
    }

    public void Close()
    {
        gameObject.SetActive(false);
        closeButton.gameObject.SetActive(false);
    }

    public bool IsOpen() => gameObject.activeSelf;
    
    public void PlayPageTurnAnimation(System.Action onComplete = null)
{
    if (isFlipping) return;
    StartCoroutine(PageTurnCoroutine(onComplete));
}

private IEnumerator PageTurnCoroutine(System.Action onComplete)
{
    isFlipping = true;
    pageTurnImage.gameObject.SetActive(true);

    foreach (Sprite sprite in pageTurnSprites)
    {
        pageTurnImage.sprite = sprite;
        yield return new WaitForSeconds(frameInterval);
    }

    // pageTurnImage.gameObject.SetActive(false);
    isFlipping = false;
    onComplete?.Invoke();
    
    pageTurnImage.sprite = defaultPageSprite;

    onComplete?.Invoke();
    isFlipping = false;
}
}
