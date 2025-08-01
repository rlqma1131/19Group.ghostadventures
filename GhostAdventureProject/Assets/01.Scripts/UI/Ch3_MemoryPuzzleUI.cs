using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_MemoryPuzzleUI : MonoBehaviour
{
    [SerializeField] private GameObject chapter1Panel;
    [SerializeField] private GameObject chapter2Panel;
    [SerializeField] private GameObject chapter3Panel;
    [SerializeField] private GameObject donePanel;

    [SerializeField] private Transform chapter1Slot;
    [SerializeField] private Transform chapter2Slot;
    [SerializeField] private Transform chapter3Slot;
    [SerializeField] private GameObject memoryNodePrefab;

    //private int maxSelections = 3;
    private List<MemoryData> selectedMemories = new();

    void Awake()
    {
        Close();
    }

    public void StartFlow(List<MemoryData> memories)
    {
        gameObject.SetActive(true);
        selectedMemories.Clear();

        chapter1Panel.SetActive(true);
        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);
        donePanel.SetActive(false);

        SetupChapter(memories, MemoryData.Chapter.Chapter1, chapter1Slot, () => OnChapterDone(chapter1Panel, chapter2Panel));
        SetupChapter(memories, MemoryData.Chapter.Chapter2, chapter2Slot, () => OnChapterDone(chapter2Panel, chapter3Panel));
        SetupChapter(memories, MemoryData.Chapter.Chapter3, chapter3Slot, () => OnChapterDone(chapter3Panel, donePanel));
    }

    void SetupChapter(List<MemoryData> all, MemoryData.Chapter chapterType, Transform slot, System.Action onCorrect)
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
            node.SetSelected(false);

            nodeMap[memory] = node;

            Button btn = go.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                if (selected.Contains(memory))
                {
                    selected.Remove(memory);
                    node.SetSelected(false);
                }
                else if (selected.Count < 3)
                {
                    selected.Add(memory);
                    node.SetSelected(true);

                    if (selected.Count == 3)
                    {
                        bool isCorrect = selected.All(m => m.isCorrectAnswer);

                        if (isCorrect)
                        {
                            selectedMemories.AddRange(selected);
                            onCorrect?.Invoke();
                        }
                        else
                        {
                            Debug.Log("틀림!");
                            StartCoroutine(ResetSelection(selected, nodeMap));
                        }
                    }
                }
            });
        }
    }

    IEnumerator ResetSelection(List<MemoryData> selected, Dictionary<MemoryData, MemoryNode> nodeMap)
    {
        yield return new WaitForSeconds(0.5f);

        foreach (var mem in selected)
        {
            if (nodeMap.TryGetValue(mem, out var node))
                node.SetSelected(false);
        }

        selected.Clear();
    }

    void OnChapterDone(GameObject current, GameObject next)
    {
        current.SetActive(false);
        next.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
        chapter1Panel.SetActive(false);
        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);
        donePanel.SetActive(false);
    }
}
