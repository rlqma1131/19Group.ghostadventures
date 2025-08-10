using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class PuzzleStatus : MonoBehaviour
{
    public enum Chapter
    {
        Chapter1,
        Chapter2,
        Chapter3,
        //Chapter4
    }

    [Header("챕터 설정")]
    [SerializeField] private Chapter currentChapter;

    [Header("PuzzleStatus UI")]
    [SerializeField] private TextMeshProUGUI clue;
    [SerializeField] private TextMeshProUGUI scannedMemory;

    [Header("각 챕터별 최대 단서 개수")]
    [SerializeField] private int maxCh1Clues = 4;
    [SerializeField] private int maxCh1Memories = 4;

    [SerializeField] private int maxCh2Clues = 4;
    [SerializeField] private int maxCh2Memories = 5;

    [SerializeField] private int maxCh3Memories = 5;

    private ChapterEndingManager ChapterEndingManager;
    void OnEnable()
    {
        // 챕터 전환 시 clue 표시/숨김 초기화
        if (currentChapter == Chapter.Chapter3) clue.gameObject.SetActive(false);
        else clue.gameObject.SetActive(true);
    }

    private void Start()
    {
        ChapterEndingManager = ChapterEndingManager.Instance;
    }

    void Update()
    {
        if (ChapterEndingManager == null) return;

        switch (currentChapter)
        {
            case Chapter.Chapter1:
                if (clue != null)
                    clue.text = $"• 최종 단서 {ChapterEndingManager.CurrentCh1ClueCount} / {maxCh1Clues}";
                if (scannedMemory != null)
                    scannedMemory.text = $"• 수집 기억 {ChapterEndingManager.CurrentCh1MemoryCount} / {maxCh1Memories}";
                break;

            case Chapter.Chapter2:
                if (clue != null)
                    clue.text = $"• 최종 단서 {ChapterEndingManager.CurrentCh2ClueCount} / {maxCh2Clues}";
                if (scannedMemory != null)
                    scannedMemory.text = $"• 수집 기억 {ChapterEndingManager.CurrentCh2MemoryCount} / {maxCh2Memories}";
                break;

            case Chapter.Chapter3:
                if (scannedMemory != null)
                    scannedMemory.text = $"• 수집 기억 {ChapterEndingManager.CurrentCh3MemoryCount} / {maxCh3Memories}";
                break;
        }
    }
}
