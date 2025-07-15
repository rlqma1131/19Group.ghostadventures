using UnityEngine;
using DG.Tweening;
using System.Collections;

public class TVShutdownEffect : MonoBehaviour
{
    [Header("오브젝트 설정")]
    [SerializeField] private SpriteRenderer tvScreen;

    [Header("속도 설정 (값이 작을수록 빨라짐)")]
    [SerializeField] private float turnOnDuration = 0.3f;
    [SerializeField] private float shutdownDurationY = 0.2f;
    [SerializeField] private float shutdownDurationX = 0.15f;
    [SerializeField] private float shutdownDurationFade = 0.1f;

    [Header("루프 설정")]
    [Tooltip("켜지고 꺼지는 동작 사이의 대기 시간 (초)")]
    [SerializeField] private float delayBetweenLoops = 1f;
    [Tooltip("체크하면 게임 시작 시 자동으로 루프를 시작합니다.")]
    [SerializeField] private bool startLoopOnPlay = true;
    [Tooltip("자동 시작 시 초기 랜덤 딜레이의 최대 시간")]
    [SerializeField] private float maxInitialDelay = 1.0f; // 초기 딜레이 시간 설정

    private Vector3 originalScale;
    private Coroutine loopingCoroutine;

    private void Awake()
    {
        if (tvScreen != null)
        {
            originalScale = tvScreen.transform.localScale;
        }
    }

    // [수정] Start를 코루틴으로 변경하여 초기 딜레이를 줌
    private IEnumerator Start()
    {
        // 자동 시작 옵션이 켜져 있을 때만 실행
        if (startLoopOnPlay)
        {
            // 0초에서 maxInitialDelay초 사이의 무작위 시간만큼 기다림
            float randomDelay = Random.Range(0f, maxInitialDelay);
            yield return new WaitForSeconds(randomDelay);

            // 랜덤 딜레이 이후에 메인 루프를 시작
            StartLooping();
        }
    }

    private float TotalShutdownDuration => shutdownDurationY + shutdownDurationX + shutdownDurationFade;

    [ContextMenu("Start Looping Effects")]
    public void StartLooping()
    {
        if (loopingCoroutine != null) StopCoroutine(loopingCoroutine);
        loopingCoroutine = StartCoroutine(EffectLoopCoroutine());
        Debug.Log($"{gameObject.name} TV 효과 반복을 시작합니다.");
    }

    [ContextMenu("Stop Looping Effects")]
    public void StopLooping()
    {
        if (loopingCoroutine != null)
        {
            StopCoroutine(loopingCoroutine);
            loopingCoroutine = null;
            Debug.Log($"{gameObject.name} TV 효과 반복을 중지합니다.");
        }
    }

    private IEnumerator EffectLoopCoroutine()
    {
        while (true)
        {
            PlayTurnOn();
            yield return new WaitForSeconds(turnOnDuration + delayBetweenLoops);

            PlayShutdown();
            yield return new WaitForSeconds(TotalShutdownDuration + delayBetweenLoops);
        }
    }

    public void PlayShutdown()
    {
        DOTween.Kill(this);
        Sequence shutdownSequence = DOTween.Sequence().SetId(this);
        shutdownSequence.Append(tvScreen.transform.DOScaleY(0.1f, shutdownDurationY).SetEase(Ease.InQuad));
        shutdownSequence.Append(tvScreen.transform.DOScaleX(0.05f, shutdownDurationX).SetEase(Ease.InQuad));
        shutdownSequence.Append(tvScreen.DOFade(0f, shutdownDurationFade));
        shutdownSequence.AppendCallback(() =>
        {
            if (tvScreen != null) tvScreen.enabled = false;
        });
    }

    public void PlayTurnOn()
    {
        DOTween.Kill(this);
        if (tvScreen != null) tvScreen.enabled = true;

        Sequence turnOnSequence = DOTween.Sequence().SetId(this);
        turnOnSequence.Join(tvScreen.DOFade(1f, turnOnDuration));
        turnOnSequence.Join(tvScreen.transform.DOScale(originalScale, turnOnDuration).SetEase(Ease.OutQuad));
    }
}