using TMPro;
using UnityEngine;

public class PuzzleStatus : MonoBehaviour
{
    //public static PuzzleStatus Instance;

    public enum Chapter { Chapter1, Chapter2, Chapter3, Chapter4 }

    [SerializeField] private Chapter currentChapter;
    [SerializeField] private TextMeshProUGUI clue;
    [SerializeField] private TextMeshProUGUI id;
    [SerializeField] private TextMeshProUGUI scannedMemory;

    [SerializeField] private int maxCh1Clues = 4, maxCh1Memories = 4;
    [SerializeField] private int maxCh2Clues = 4, maxCh2Memories = 5;
    [SerializeField] private int maxCh3Memories = 5;

    private ChapterEndingManager chapterEndingManager => ChapterEndingManager.Instance;

    private void OnEnable()
    {
        ChapterEndingManager.Instance.ProgressChanged += Refresh;
        Refresh(); // 지금까지 누적된 런타임 상태 그대로 표시
    }
    private void OnDisable()
    {
        if (ChapterEndingManager.Instance != null)
            ChapterEndingManager.Instance.ProgressChanged -= Refresh;
    }

    public void Refresh()
    {
        if (chapterEndingManager == null) return;

        switch (currentChapter)
        {
            case Chapter.Chapter1:
                var ordered = chapterEndingManager.GetClueIdsOrdered(1);
                if (clue) clue.text = $"• 최종 단서 {chapterEndingManager.CurrentCh1ClueCount} / {maxCh1Clues}";
                if (clue) id.text = string.Join(", ", chapterEndingManager.GetClueIdsOrdered(1));
                if (scannedMemory) scannedMemory.text = $"• 기억 수집 {chapterEndingManager.CurrentCh1MemoryCount} / {maxCh1Memories}";
                break;

            case Chapter.Chapter2:
                if (clue) clue.text = $"• 최종 단서 {chapterEndingManager.CurrentCh2ClueCount} / {maxCh2Clues}";
                if (clue) id.text = string.Join(", ", chapterEndingManager.GetClueIdsOrdered(2));
                if (scannedMemory) scannedMemory.text = $"• 기억 수집 {chapterEndingManager.CurrentCh2MemoryCount} / {maxCh2Memories}";
                break;

            case Chapter.Chapter3:
                if (scannedMemory) scannedMemory.text = $"• 기억 수집 {chapterEndingManager.CurrentCh3MemoryCount} / {maxCh3Memories}";
                break;
            
            case Chapter.Chapter4:
                if (scannedMemory) scannedMemory.text = "• 기억 수집 ? / ?";
                break;
        }

        Debug.Log($"[PuzzleStatus] 현재 챕터 : {currentChapter} / 최종 단서 : {chapterEndingManager.CurrentCh1ClueCount} / 수집 기억 : {chapterEndingManager.CurrentCh1MemoryCount}");
    }
}
