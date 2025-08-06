using UnityEngine;
using DG.Tweening;

public class KeyTutorial : MonoBehaviour
{
    [SerializeField] CanvasGroup canvasGroup;
    public float duration = 1f;

    public float waitTime = 2.0f; // UI가 화면에 머무는 시간 (1초)

    void Start()
    {
        gameObject.SetActive(false);
    }

    void OnEnable()
    {
        FadeInAndOut();
    }

    public void FadeInAndOut()
    {   
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
