using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Ch3_MemoryPuzzleUI : MonoBehaviour
{
    [Header("판넬들")]
    [SerializeField] private GameObject chapter1Panel;
    [SerializeField] private GameObject chapter2Panel;
    [SerializeField] private GameObject chapter3Panel;

    [Header("슬롯 래퍼")]
    [SerializeField] private Transform chapter1SlotWrapper;
    [SerializeField] private Transform chapter2SlotWrapper;
    [SerializeField] private Transform chapter3SlotWrapper;

    [Header("기타 셋팅")]
    [SerializeField] private GameObject memoryNodePrefab;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private CanvasGroup overallCanvasGroup;
    [SerializeField] private float uiFadeDuration = 0.5f;
    [SerializeField] private int maxColumnsPerRow = 4;

    [Header("문 & 스캐너")]
    [SerializeField] private Ch3_ClearDoor clearDoor;
    [SerializeField] private Ch3_Scanner scanner;

    private List<MemoryData> selectedMemories = new();
    private bool isInteractable = true;
    public bool puzzlecompleted = false;

    [SerializeField] private MemoryData memoryData;

    void Awake() => Close();

    public void StartFlow(List<MemoryData> memories)
    {
        gameObject.SetActive(true);
        selectedMemories.Clear();
        overallCanvasGroup.alpha = 0;

        chapter1Panel.SetActive(true);
        chapter2Panel.SetActive(true);
        chapter3Panel.SetActive(true);

        SetupChapter(memories, MemoryData.Chapter.Chapter1, chapter1SlotWrapper, chapter1Panel, chapter2Panel);
        SetupChapter(memories, MemoryData.Chapter.Chapter2, chapter2SlotWrapper, chapter2Panel, chapter3Panel);
        SetupChapter(memories, MemoryData.Chapter.Chapter3, chapter3SlotWrapper, chapter3Panel, null);

        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);

        StartCoroutine(FadeCanvas(overallCanvasGroup, 0f, 1f, uiFadeDuration));
    }

    void SetupChapter(List<MemoryData> all,
                      MemoryData.Chapter chapterType,
                      Transform slotWrapper,
                      GameObject currentPanel,
                      GameObject nextPanel)
    {
        foreach (Transform t in slotWrapper)
            Destroy(t.gameObject);

        var chapterMemories = all.FindAll(m => m.chapter == chapterType);
        List<MemoryData> selected = new();
        Dictionary<MemoryData, MemoryNode> nodeMap = new();

        GameObject currentRow = null;
        int columnCount = 0;

        foreach (var memory in chapterMemories)
        {
            if (columnCount == 0 || columnCount >= maxColumnsPerRow)
            {
                currentRow = Instantiate(rowPrefab, slotWrapper);
                columnCount = 0;
            }

            var go = Instantiate(memoryNodePrefab, currentRow.transform);
            go.name = $"MemoryNode_{memory.memoryID}";
            var node = go.GetComponent<MemoryNode>();
            node.Initialize(memory);
            node.SetStateEffect(MemoryState.None);

            nodeMap[memory] = node;
            columnCount++;

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

                            if (nextPanel != null)
                            {
                                StartCoroutine(CorrectEffect(selectedNodes, currentPanel, nextPanel));
                            }
                            else
                            {
                                StartCoroutine(CompletePuzzleFlow(selectedNodes, currentPanel));
                            }
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

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotWrapper.GetComponent<RectTransform>());
    }

    IEnumerator CorrectEffect(List<MemoryNode> nodes, GameObject currentPanel, GameObject nextPanel)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Correct);

        yield return new WaitForSeconds(1f);

        var currentGroup = currentPanel.GetComponent<CanvasGroup>();
        if (currentGroup != null)
            yield return FadeCanvas(currentGroup, 1f, 0f, 0.5f);

        currentPanel.SetActive(false);

        nextPanel.SetActive(true);
        var nextGroup = nextPanel.GetComponent<CanvasGroup>();
        if (nextGroup != null)
        {
            nextGroup.alpha = 0;
            yield return FadeCanvas(nextGroup, 0f, 1f, 0.5f);
        }

        isInteractable = true;
    }

    IEnumerator CompletePuzzleFlow(List<MemoryNode> nodes, GameObject currentPanel)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Correct);

        yield return new WaitForSeconds(1f);

        var currentGroup = currentPanel.GetComponent<CanvasGroup>();
        if (currentGroup != null)
            yield return FadeCanvas(currentGroup, 1f, 0f, 0.5f);

        currentPanel.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        yield return FadeCanvas(overallCanvasGroup, 1f, 0f, uiFadeDuration);

        //페이드인
        CutsceneManager.Instance.StartCoroutine(CutsceneManager.Instance.PlayCutscene());
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Ch03_End", LoadSceneMode.Additive);
        Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>(); // 플레이어 오브젝트 설정

        MemoryManager.Instance.TryCollect(memoryData); // 기억 조각 수집

        clearDoor.OpenDoor();
        gameObject.SetActive(false);
        puzzlecompleted = true;

        SaveData data = new SaveData
        {
            checkpointId = "3챕터 최종 퍼즐",
            sceneName = "Ch03_Hospital",
            playerPosition = GameManager.Instance.Player.transform.position,


            collectedClueNames = _inventory.collectedClues.Select(c => c.clue_Name).ToList(),

            collectedMemoryIDs = MemoryManager.Instance.collectedMemoryIDs.ToList(),

            scannedMemoryTitles = MemoryManager.Instance.ScannedMemories.Select(m => m.memoryTitle).ToList()
        };

        SaveManager.SaveGame(data);
        yield return new WaitForSeconds(0.5f);
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
