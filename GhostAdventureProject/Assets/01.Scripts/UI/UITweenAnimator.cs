using UnityEngine;
using DG.Tweening;
using UnityEngine.UI;
using TMPro;

public class UITweenAnimator : MonoBehaviour
{
    //public RectTransform uiElement;
    // public CanvasGroup canvasGroup; // CanvasGroup 컴포넌트 

    public TextMeshProUGUI text;
    public float duration = 0.5f;
    //public float startX = -788f;
    //public float endX = 0f;

    
    public float waitTime = 1.0f; // UI가 화면에 머무는 시간 (1초)

    //public void SlideInAndOut()
    //{
    //    //초기 위치 설정

    //    Vector2 startPos = new Vector2(startX, uiElement.anchoredPosition.y);
    //    uiElement.anchoredPosition = startPos;

        
    //    Sequence sequence = DOTween.Sequence();

    //    //슬라이드 인 애니메이션
    //    sequence.Append(
    //        uiElement.DOAnchorPosX(endX, duration)
    //                 .SetEase(Ease.OutQuad)
    //    );

    //    //  대기 시간
    //    sequence.AppendInterval(waitTime);

    //    // 슬라이드 아웃 애니메이션
    //    sequence.Append(
    //        uiElement.DOAnchorPosX(startX, duration)
    //                 .SetEase(Ease.InQuad) 
    //    );

        
    //    sequence.Play();
    //}
        public void FadeInAndOut()
    {
        if (text == null)
        {
            Debug.LogError("CanvasGroup이 할당되지 않았습니다.");
            return;
        }


        Sequence sequence = DOTween.Sequence();


        sequence.Append(
            text.DOFade(1f, duration)
                       .SetEase(Ease.Linear) // 또는 Ease.InQuad
        );

        sequence.AppendInterval(waitTime);


        sequence.Append(
            text.DOFade(0f, duration)
                       .SetEase(Ease.Linear) // 또는 Ease.OutQuad
        );


        sequence.Play();
    }

}