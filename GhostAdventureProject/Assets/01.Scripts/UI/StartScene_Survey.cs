using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;

public class StartScene_Surveyh : MonoBehaviour
{
    private Image targetImage;
    private Sequence rainbowSequence;

    private void Awake()
    {
        targetImage = GetComponent<Image>();
    }

    private void Start()
    {
        Vector3 targetScale = transform.localScale * 1.2f;

        transform.DOScale(targetScale, 0.5f)
            .SetLoops(-1, LoopType.Yoyo)
            .SetEase(Ease.InOutSine)
            .SetLink(gameObject); // 오브젝트 Destroy 시 자동 Kill

        StartRainbowColorLoop();
    }

    void StartRainbowColorLoop()
    {
        if (targetImage == null)
        {
            Debug.LogWarning("targetImage가 할당되지 않았습니다!");
            return;
        }

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

        rainbowSequence = DOTween.Sequence();

        foreach (Color color in rainbowColors)
        {
            rainbowSequence.Append(
                targetImage.DOColor(color, durationPerColor)
                .SetLink(gameObject) // 오브젝트 Destroy 시 자동 Kill
            );
        }

        rainbowSequence.SetLoops(-1)
            .SetLink(gameObject);
    }

    private void OnDestroy()
    {
        rainbowSequence?.Kill(); // 혹시 모를 잔여 트윈 수동 종료
    }
}
