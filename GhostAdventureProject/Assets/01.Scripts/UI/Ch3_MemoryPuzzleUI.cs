using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_MemoryPuzzleUI : MonoBehaviour
{
    [Header("판넬들")]
    [SerializeField] private GameObject chapter1Panel;
    [SerializeField] private GameObject chapter2Panel;
    [SerializeField] private GameObject chapter3Panel;

    [Header("슬롯들")]
    [SerializeField] private Transform chapter1Slot;
    [SerializeField] private Transform chapter2Slot;
    [SerializeField] private Transform chapter3Slot;

    [Header("기타 셋팅")]
    [SerializeField] private GameObject memoryNodePrefab;
    [SerializeField] private GameObject Skip;
    [SerializeField] private CanvasGroup overallCanvasGroup;
    [SerializeField] private float uiFadeDuration = 0.5f;

    private List<MemoryData> selectedMemories = new();
    private bool isInteractable = true;

    void Awake()
    {
        Close();
    }

    public void StartFlow(List<MemoryData> memories)
    {
        gameObject.SetActive(true);
        selectedMemories.Clear();

        overallCanvasGroup.alpha = 0;
        chapter1Panel.SetActive(false);
        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);

        // 연출 후 등장
        StartCoroutine(StartUIRoutine(memories));
    }
    IEnumerator StartUIRoutine(List<MemoryData> memories)
    {
        yield return StartCoroutine(FadeCanvas(overallCanvasGroup, 0f, 1f, uiFadeDuration));

        chapter1Panel.SetActive(true);
        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);

        SetupChapter(memories, MemoryData.Chapter.Chapter1, chapter1Slot, chapter1Panel, chapter2Panel);
        SetupChapter(memories, MemoryData.Chapter.Chapter2, chapter2Slot, chapter2Panel, chapter3Panel);
        SetupChapter(memories, MemoryData.Chapter.Chapter3, chapter3Slot, chapter3Panel, null);

        Button skipButton = Skip.GetComponent<Button>();
        skipButton.onClick.RemoveAllListeners();
        skipButton.onClick.AddListener(() => OnClickSkip());
    }

    void SetupChapter(List<MemoryData> all,
                      MemoryData.Chapter chapterType,
                      Transform slot,
                      GameObject currentPanel,
                      GameObject nextPanel)
    {
        foreach (Transform t in slot) Destroy(t.gameObject);

        var chapterMemories = all.FindAll(m => m.chapter == chapterType);
        List<MemoryData> selected = new();
        Dictionary<MemoryData, MemoryNode> nodeMap = new();

        foreach (var memory in chapterMemories)
        {
            var go = Instantiate(memoryNodePrefab, slot);
            go.name = $"MemoryNode_{memory.memoryID}";
            var node = go.GetComponent<MemoryNode>();
            node.Initialize(memory);
            node.SetStateEffect(MemoryState.None);

            nodeMap[memory] = node;

            Button btn = go.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                if (!isInteractable) return;

                if (selected.Contains(memory))
                {
                    selected.Remove(memory);
                    node.SetStateEffect(MemoryState.None);
                }
                else if (selected.Count < 3)
                {
                    selected.Add(memory);
                    node.SetStateEffect(MemoryState.Selected);

                    if (selected.Count == 3)
                    {
                        var selectedNodes = selected.Select(m => nodeMap[m]).ToList();

                        if (selected.All(m => m.isCorrectAnswer))
                        {
                            selectedMemories.AddRange(selected);
                            StartCoroutine(CorrectEffect(selectedNodes, currentPanel, nextPanel));
                        }
                        else
                        {
                            Debug.Log("틀림!");
                            StartCoroutine(WrongEffect(selectedNodes));
                            selected.Clear();
                        }
                    }
                }
            });
        }
    }

    private void OnClickSkip()
    {
        if (!isInteractable) return;
        isInteractable = false;

        List<MemoryNode> chapter3Nodes = new();
        foreach (Transform t in chapter3Slot)
        {
            if (t.TryGetComponent<MemoryNode>(out var node))
                chapter3Nodes.Add(node);
        }

        StartCoroutine(SkipFinalEffect(chapter3Nodes, chapter3Panel));
    }

    IEnumerator CorrectEffect(List<MemoryNode> nodes, GameObject currentPanel, GameObject nextPanel)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Correct);

        yield return new WaitForSeconds(1f);

        CanvasGroup currentGroup = currentPanel.GetComponent<CanvasGroup>();
        if (currentGroup != null)
            yield return StartCoroutine(FadeCanvas(currentGroup, 1f, 0f, 0.5f));

        currentPanel.SetActive(false);

        if (nextPanel != null)
        {
            nextPanel.SetActive(true);

            CanvasGroup nextGroup = nextPanel.GetComponent<CanvasGroup>();
            if (nextGroup != null)
            {
                nextGroup.alpha = 0f;
                yield return StartCoroutine(FadeCanvas(nextGroup, 0f, 1f, 0.5f));
            }

            isInteractable = true;
        }
        else
        {
            // 마지막 챕터면 전체 UI 종료
            yield return new WaitForSeconds(0.5f);
            yield return StartCoroutine(FadeCanvas(overallCanvasGroup, 1f, 0f, uiFadeDuration));
            gameObject.SetActive(false);
        }
    }

    IEnumerator WrongEffect(List<MemoryNode> nodes)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Wrong);

        yield return new WaitForSeconds(2f);

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.None);

        isInteractable = true;
    }

    IEnumerator SkipFinalEffect(List<MemoryNode> nodes, GameObject currentPanel)
    {
        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Correct);

        yield return new WaitForSeconds(1f);

        CanvasGroup currentGroup = currentPanel.GetComponent<CanvasGroup>();
        if (currentGroup != null)
            yield return StartCoroutine(FadeCanvas(currentGroup, 1f, 0f, 0.5f));

        currentPanel.SetActive(false);
        gameObject.SetActive(false);

        yield return new WaitForSeconds(0.5f);
        yield return StartCoroutine(FadeCanvas(overallCanvasGroup, 1f, 0f, uiFadeDuration));

        gameObject.SetActive(false);
    }

    IEnumerator FadeCanvas(CanvasGroup group, float from, float to, float duration)
    {
        float time = 0;
        while (time < duration)
        {
            time += Time.deltaTime;
            group.alpha = Mathf.Lerp(from, to, time / duration);
            yield return null;
        }
        group.alpha = to;
    }

    public void Close()
    {
        gameObject.SetActive(false);
        chapter1Panel.SetActive(false);
        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);
    }
}
