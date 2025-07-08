using DG.Tweening;
using UnityEngine;

public class Ch1_Cat : MoveBasePossessable
{
    protected override void Start()
    {
        base.Start();
        hasActivated = false;
    }

    protected override void Update()
    {
        if (!hasActivated)
            return;

        base.Update();
        if (Input.GetKeyDown(KeyCode.Q))
        {
            CatAct();
        }
        // 점프 추가
        //else if (Input.GetKeyDown(KeyCode.Space))
        //{
        //    Jump();
        //}
    }

    public void Blink()
    {
        anim.SetTrigger("Blink");
    }

    public void ActivateCat()
    {
        // 1. 점프 애니메이션
        float jumpHeight = 1.5f;
        float jumpDuration = 0.4f;

        // 현재 위치 저장
        Vector3 originalPos = transform.position;

        // 2. 점프 시퀀스
        Sequence jumpSequence = DOTween.Sequence();

        // 위로 점프
        jumpSequence.Append(transform.DOMoveY(originalPos.y + jumpHeight, jumpDuration * 0.5f).SetEase(Ease.OutQuad));

        // 아래로 착지
        jumpSequence.Append(transform.DOMoveY(originalPos.y, jumpDuration * 0.5f).SetEase(Ease.InQuad));

        // 3. 착지 후 Idle 애니메이션으로 전환
        jumpSequence.AppendCallback(() =>
        {
            hasActivated = true;
            anim.SetBool("Idle", true);
        });
    }

    void CatAct()
    {
        // 문열기
        anim.SetTrigger("Open");
    }
}
