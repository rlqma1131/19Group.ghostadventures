using UnityEngine;
using DG.Tweening;

public class Ch04_DoScale : MonoBehaviour
{
    //챕4 선택지 떨리게


    public float duration = 0.1f;     // 한 번 떨리는 시간
    public float strength = 0.5f;       // 떨림 강도
    public int vibrato = 50;          // 진동 횟수
    public float randomness = 90f;    // 랜덤 방향
    public bool fadeOut = false;      // 점점 약해지게 할지 여부

    private Tween shakeTween;

    void Start()
    {
        // 기존 트윈 있으면 제거
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();


        shakeTween = transform.DOShakePosition(duration, strength, vibrato, randomness, fadeOut)
                              .SetLoops(-1, LoopType.Restart)
                              .SetEase(Ease.Linear);
    }

    void OnDisable()
    {
        // 비활성화될 때 트윈 정리
        if (shakeTween != null && shakeTween.IsActive())
            shakeTween.Kill();
    }
}
