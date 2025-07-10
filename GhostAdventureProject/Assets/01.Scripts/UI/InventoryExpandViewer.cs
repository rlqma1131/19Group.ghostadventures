using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryExpandViewer : MonoBehaviour
{
    [SerializeField] private GameObject cluePanel; // 단서 뷰어 패널(검은 배경)
    [SerializeField] private Image clueImage; // 단서 이미지
    [SerializeField] private TextMeshProUGUI clueName; // 단서 이름
    [SerializeField] private TextMeshProUGUI clueDescription; // 단서 설명
    private bool isShowing = false; // "패널 클릭시 닫기" 기능용 불값

    public static InventoryExpandViewer Instance; // 싱글톤(수정예정)

    private void Awake()
    {
        Instance = this;
        cluePanel.SetActive(false);
    }

    // 단서 크게 보여주기
    public void ShowClue(ClueData clue)
    {
        clueImage.sprite = clue.clue_Image;
        clueName.text = clue.clue_Name;
        clueDescription.text = clue.clue_Description;
        clueImage.SetNativeSize();

        // 이미지가 클 경우 크기 조정
        float originalHeight = clue.clue_Image.texture.height;
        if(originalHeight > 800f)
        {
            float fixedHeight = 800;
            float originalWidth = clue.clue_Image.texture.width;
            float aspectRatio = originalWidth / originalHeight;
            float calculatedWidth = fixedHeight * aspectRatio;

            clueImage.rectTransform.sizeDelta = new Vector2(calculatedWidth, fixedHeight);

            cluePanel.SetActive(true);
            isShowing = true;
        }
    }

    // 단서패널 닫기
    public void HideClue()
    {
        cluePanel.SetActive(false);
        isShowing = false;
    }

    public bool IsShowing() => isShowing;  
}
