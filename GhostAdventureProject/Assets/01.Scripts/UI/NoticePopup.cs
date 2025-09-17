using UnityEngine;
using TMPro;
using DG.Tweening;

// 알림팝업 스크립트입니다.
// 화면 좌측 상단(영혼에너지 아래)에 플레이에 도움을 주는 내용을 표시할 때 사용합니다.
public class NoticePopup : MonoBehaviour
{
    public TextMeshProUGUI text;
    [SerializeField] CanvasGroup canvasGroup;
    public float duration = 1f;

    public float waitTime = 2.0f; // UI가 화면에 머무는 시간 (1초)

    void Start()
    {
        gameObject.SetActive(false);
    }
    
    public void FadeInAndOut(string notice)
    {   
        if (canvasGroup == null || text == null)
        {
            Debug.LogError("CanvasGroup 또는 Text가 할당되지 않았습니다.");
            return;
        }
        text.text = notice;

        canvasGroup.alpha = 0f;
        canvasGroup.gameObject.SetActive(true);

        Sequence sequence = DOTween.Sequence();

        sequence.Append(canvasGroup.DOFade(1f, duration).SetEase(Ease.InOutSine));
        sequence.AppendInterval(waitTime);
        sequence.Append(canvasGroup.DOFade(0f, duration).SetEase(Ease.InOutSine));
        sequence.OnComplete(() => canvasGroup.gameObject.SetActive(false));

        sequence.Play();
    }

}