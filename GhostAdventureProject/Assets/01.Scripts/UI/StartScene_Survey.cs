using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class StartScene_Surveyh : MonoBehaviour
{

    private Image targetImage;


    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }
    private void Start()
    {
        
        
        Vector3  targetScale = transform.localScale * 1.2f;

        transform.DOScale(targetScale, 0.5f).SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine);




        StartRainbowColorLoop();


    }

    void StartRainbowColorLoop()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("targetImage가 할당되지 않았습니다!");
            return;
        }

        // 무지개 색상 배열 (빨, 주, 노, 초, 파, 남, 보)
        Color[] rainbowColors = new Color[]
        {
            Color.red,
            new Color(1f, 0.5f, 0f), // 주황
            Color.yellow,
            Color.green,
            Color.cyan,
            Color.blue,
            new Color(0.5f, 0f, 1f) // 보라
        };

        float durationPerColor = 0.5f;

        Sequence rainbowSequence = DOTween.Sequence();

        foreach (Color color in rainbowColors)
        {
            rainbowSequence.Append(targetImage.DOColor(color, durationPerColor));
        }

        rainbowSequence.SetLoops(-1);
    }

}
