using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Pan : BasePossessable
{
    [SerializeField] private AudioClip isFall;

    protected override void Start()
    {
        base.Start();
        isCompleted = true;
        // 초기화 작업이 필요하다면 여기에 추가
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerPanEvent();
        }
    }

    private void TriggerPanEvent()
    {
        hasActivated = true;

        // 초기값 저장
        Vector3 originalPos = transform.localPosition;
        Quaternion originalRot = transform.localRotation;

        // 애니메이션 시퀀스 생성
        Sequence panSequence = DOTween.Sequence();

        // 1. 오른쪽으로 기울이며 아래로 떨어짐 (0.3초)
        panSequence.Append(transform.DOLocalRotate(new Vector3(0f, 0f, -60f), 0.3f).SetEase(Ease.InQuad));
        panSequence.Join(transform.DOLocalMoveY(originalPos.y - 1.5f, 0.3f).SetEase(Ease.InQuad));

        // 2. 사운드 재생
        panSequence.AppendCallback(() =>
        {
            SoundManager.Instance.PlaySFX(isFall);
        });

        // 3. 회전 원래대로 복귀
        panSequence.Append(transform.DOLocalRotateQuaternion(originalRot, 0.2f).SetEase(Ease.OutBounce));
    }


}
