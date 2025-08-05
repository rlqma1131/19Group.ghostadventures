using DG.Tweening;
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

    [Header("선택 슬롯 셋팅")]
    [SerializeField] private Transform clickedSlotWrapper;
    [SerializeField] private GameObject clickedSlotPrefab;
    [SerializeField] private GameObject clickedSlotRowPrefab;
    [SerializeField] private int maxClickedPerRow = 8;

    [Header("기타 셋팅")]
    [SerializeField] private GameObject memoryNodePrefab;
    [SerializeField] private GameObject rowPrefab;
    [SerializeField] private Button nextBtn;
    [SerializeField] private CanvasGroup overallCanvasGroup;
    [SerializeField] private float uiFadeDuration = 0.5f;
    [SerializeField] private int maxColumnsPerRow = 4;

    [Header("문 & 스캐너")]
    [SerializeField] private Ch3_ClearDoor clearDoor;
    [SerializeField] private Ch3_Scanner scanner;

    private List<MemoryData> selectedMemories = new();
    private List<MemoryData> currentSelected = new();
    private Dictionary<MemoryData, MemoryNode> currentNodeMap = new();
    private List<MemoryData> currentChapterMemories = new();
    private GameObject currentPanel = null;
    private GameObject nextPanel = null;

    private GameObject currentClickedRow;
    private int clickedColumnCount = 0;
    private List<GameObject> clickedSlots = new();
    private List<MemoryData> allMemories;

    private bool isInteractable = true;
    public bool puzzlecompleted = false;

    [SerializeField] private MemoryData memoryData;

    void Awake() => Close();

    public void StartFlow(List<MemoryData> memories)
    {
        allMemories = memories;

        gameObject.SetActive(true);
        selectedMemories.Clear();
        overallCanvasGroup.alpha = 0;

        chapter1Panel.SetActive(true);
        chapter2Panel.SetActive(true);
        chapter3Panel.SetActive(true);

        SetupChapter(memories, MemoryData.Chapter.Chapter1, chapter1SlotWrapper, chapter1Panel, chapter2Panel, GetColumnsPerRow(MemoryData.Chapter.Chapter1));

        chapter2Panel.SetActive(false);
        chapter3Panel.SetActive(false);

        nextBtn.onClick.RemoveAllListeners();
        nextBtn.onClick.AddListener(CheckAnswer);

        StartCoroutine(FadeCanvas(overallCanvasGroup, 0f, 1f, uiFadeDuration));
    }

    void SetupChapter(List<MemoryData> all,
                  MemoryData.Chapter chapterType,
                  Transform slotWrapper,
                  GameObject panel,
                  GameObject next,
                  int columnsPerRow)
    {
        foreach (Transform t in slotWrapper)
            Destroy(t.gameObject);

        currentSelected = new();
        currentNodeMap = new();
        currentChapterMemories = all.FindAll(m => m.chapter == chapterType);
        currentPanel = panel;
        nextPanel = next;

        GameObject currentRow = null;
        int columnCount = 0;

        foreach (var memory in currentChapterMemories)
        {
            if (columnCount == 0 || columnCount >= columnsPerRow)
            {
                currentRow = Instantiate(rowPrefab, slotWrapper);
                columnCount = 0;
            }

            var go = Instantiate(memoryNodePrefab, currentRow.transform);
            go.name = $"MemoryNode_{memory.memoryID}";
            var node = go.GetComponent<MemoryNode>();
            node.Initialize(memory);
            node.SetStateEffect(MemoryState.None);

            currentNodeMap[memory] = node;
            columnCount++;

            Button btn = go.GetComponentInChildren<Button>();
            btn.onClick.AddListener(() =>
            {
                if (!isInteractable) return;

                if (currentSelected.Contains(memory))
                {
                    currentSelected.Remove(memory);
                    node.SetStateEffect(MemoryState.None);

                    var slotToRemove = clickedSlots.FirstOrDefault(slot => slot.name == $"ClickedSlot_{memory.memoryID}");
                    if (slotToRemove != null)
                    {
                        clickedSlots.Remove(slotToRemove);
                        Destroy(slotToRemove);
                        clickedColumnCount = Mathf.Max(0, clickedColumnCount - 1);
                    }
                }
                else
                {
                    currentSelected.Add(memory);
                    node.SetStateEffect(MemoryState.Selected);

                    if (currentClickedRow == null || clickedColumnCount >= maxClickedPerRow)
                    {
                        currentClickedRow = Instantiate(clickedSlotRowPrefab, clickedSlotWrapper);
                        clickedColumnCount = 0;
                    }

                    var clickedUI = Instantiate(clickedSlotPrefab, currentClickedRow.transform);
                    clickedUI.name = $"ClickedSlot_{memory.memoryID}";
                    clickedSlots.Add(clickedUI);
                    clickedColumnCount++;

                    var image = clickedUI.GetComponentInChildren<Image>();
                    if (image != null && memory.MemoryCutSceneImage != null)
                    {
                        image.sprite = memory.MemoryCutSceneImage;
                    }
                }
            });
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(slotWrapper.GetComponent<RectTransform>());
    }

    void CheckAnswer()
    {
        if (!isInteractable) return;

        var correctAnswers = currentChapterMemories.Where(m => m.isCorrectAnswer).ToList();
        var selectedNodes = currentSelected
            .Where(m => currentNodeMap.ContainsKey(m))
            .Select(m => currentNodeMap[m])
            .ToList();

        bool allCorrectSelected =
            correctAnswers.All(c => currentSelected.Contains(c)) &&
            currentSelected.All(s => s.isCorrectAnswer);

        if (allCorrectSelected)
        {
            selectedMemories.AddRange(correctAnswers);

            foreach (var correct in correctAnswers)
            {
                if (!selectedMemories.Contains(correct))
                    selectedMemories.Add(correct);
            }

            if (nextPanel != null)
                StartCoroutine(CorrectEffect(selectedNodes, currentPanel, nextPanel));
            else
                StartCoroutine(CompletePuzzleFlow(selectedNodes, currentPanel));
        }
        else
        {
            Debug.Log("틀림!");

            var roundClicked = currentSelected
                .Select(m => clickedSlots.FirstOrDefault(slot => slot.name == $"ClickedSlot_{m.memoryID}"))
                .Where(slot => slot != null)
                .ToList();

            StartCoroutine(WrongEffect(selectedNodes, roundClicked));
            currentSelected.Clear();
        }
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

        MemoryData.Chapter nextChapter = GetNextChapter();
        Transform nextSlotWrapper = GetSlotWrapper(nextChapter);


        SetupChapter(allMemories, nextChapter, nextSlotWrapper, nextPanel, GetNextPanel(nextChapter), GetColumnsPerRow(nextChapter));

        isInteractable = true;
    }

    int GetColumnsPerRow(MemoryData.Chapter chapter)
    {
        return chapter switch
        {
            MemoryData.Chapter.Chapter1 => 4,
            MemoryData.Chapter.Chapter2 => 3,
            MemoryData.Chapter.Chapter3 => 3,
            _ => maxColumnsPerRow
        };
    }

    MemoryData.Chapter GetNextChapter()
    {
        if (currentPanel == chapter1Panel) return MemoryData.Chapter.Chapter2;
        if (currentPanel == chapter2Panel) return MemoryData.Chapter.Chapter3;
        return MemoryData.Chapter.Chapter3;
    }

    Transform GetSlotWrapper(MemoryData.Chapter chapter)
    {
        return chapter switch
        {
            MemoryData.Chapter.Chapter1 => chapter1SlotWrapper,
            MemoryData.Chapter.Chapter2 => chapter2SlotWrapper,
            MemoryData.Chapter.Chapter3 => chapter3SlotWrapper,
            _ => null
        };
    }

    GameObject GetNextPanel(MemoryData.Chapter chapter)
    {
        return chapter switch
        {
            MemoryData.Chapter.Chapter1 => chapter2Panel,
            MemoryData.Chapter.Chapter2 => chapter3Panel,
            MemoryData.Chapter.Chapter3 => null,
            _ => null
        };
    }

    IEnumerator CompletePuzzleFlow(List<MemoryNode> nodes, GameObject currentPanel)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Correct);

        yield return new WaitForSeconds(1f);

        foreach (var slot in clickedSlots)
        {
            // 진동
            if (slot != null)
                StartCoroutine(ShakeIntensifying(slot.transform, 0.8f, 25f)); // 0.8초간 점점 세게
        }

        yield return new WaitForSeconds(0.8f);

        var currentGroup = currentPanel.GetComponent<CanvasGroup>();
        if (currentGroup != null)
            yield return FadeCanvas(currentGroup, 1f, 0f, 0.5f);

        currentPanel.SetActive(false);
        yield return new WaitForSeconds(0.5f);
        yield return FadeCanvas(overallCanvasGroup, 1f, 0f, uiFadeDuration);

        CutsceneManager.Instance.StartCoroutine(CutsceneManager.Instance.PlayCutscene());
        yield return new WaitForSeconds(2f);
        SceneManager.LoadScene("Ch03_End", LoadSceneMode.Additive);

        Inventory_Player _inventory = GameManager.Instance.Player.GetComponent<Inventory_Player>();
        MemoryManager.Instance.TryCollect(memoryData);

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

    // 정답 진동 효과
    IEnumerator ShakeIntensifying(Transform target, float duration = 0.8f, float maxMagnitude = 20f)
    {
        Vector3 originalPos = target.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float magnitude = Mathf.Lerp(2f, maxMagnitude, progress); // 점점 강해지는 진동
            float x = Random.Range(-1f, 1f) * magnitude;
            float y = Random.Range(-1f, 1f) * magnitude * 0.3f;

            target.localPosition = originalPos + new Vector3(x, y, 0f);

            elapsed += Time.deltaTime;
            yield return null;
        }

        target.localPosition = originalPos;
    }

    IEnumerator WrongEffect(List<MemoryNode> nodes, List<GameObject> roundClickedSlots)
    {
        isInteractable = false;

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.Wrong);

        yield return new WaitForSeconds(2f);

        foreach (var node in nodes)
            node.SetStateEffect(MemoryState.None);

        foreach (var slot in roundClickedSlots)
        {
            if (clickedSlots.Contains(slot))
                clickedSlots.Remove(slot);
            Destroy(slot);
        }

        currentClickedRow = null;
        clickedColumnCount = 0;

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
