using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 기억저장소 챕터버튼 스크립트입니다.
// 버튼을 클릭하면 해당 챕터의 기억들만 표시됩니다.
public class MemoryStorage_ChapterButton : MonoBehaviour
{
    [SerializeField] private MemoryStorage memorystorage;
    [SerializeField] private List<Button> chapterButtons; // Ch1, Ch2, Ch3 순서대로
    [SerializeField] private Color selectedColor = new Color(0.2f,0.7f,1f,1f);
    [SerializeField] private Color normalColor   = Color.white;

    private int currentIndex = 0;

    void Awake()
    {
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            int idx = i;
            chapterButtons[i].onClick.AddListener(() => OnClickChapter(idx));
        }
        RefreshVisuals();
    }

    void OnClickChapter(int index)
    {
        currentIndex = index;

        var chapter = (MemoryData.Chapter)index; // 버튼 순서를 enum 순서와 맞춘다
        memorystorage.SetChapter(chapter);
        RefreshVisuals();
    }

    void RefreshVisuals()
    {
        for (int i = 0; i < chapterButtons.Count; i++)
        {
            var btn = chapterButtons[i];

            // 1) 선택된 탭은 클릭 불가로(탭 느낌)
            btn.interactable = i != currentIndex;

            // 2) 색상/이미지 변경
            var img = btn.GetComponent<Image>();
            if (img) img.color = (i == currentIndex) ? selectedColor : normalColor;

            // 3) 텍스트 색 바꾸고 싶으면
            var label = btn.GetComponentInChildren<TMP_Text>();
            if (label) label.alpha = (i == currentIndex) ? 1f : 0.8f;
        }
    }
}
