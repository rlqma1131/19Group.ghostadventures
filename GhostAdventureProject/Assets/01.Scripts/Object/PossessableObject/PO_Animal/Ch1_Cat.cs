using Cinemachine;
using DG.Tweening;
using System.Collections;
using UnityEngine;

public class Ch1_Cat : MoveBasePossessable
{
    [SerializeField] private LockedDoor door;
    private bool isNearDoor = false;

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

        if (Input.GetKeyDown(KeyCode.Q) && isNearDoor)
        {
            // 문 열기
            StartCoroutine(CatAct());
        }
    }

    // 문 근처에 있는지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<LockedDoor>() == door)
        {
            isNearDoor = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D (collision);

        if (collision.GetComponent<LockedDoor>() == door)
        {
            isNearDoor = false;
        }
    }

    public void Blink()
    {
        anim.SetTrigger("Blink");
    }

    public void ActivateCat()
    {
        // 1. 점프 애니메이션
        anim.SetTrigger("Surprise");
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

    IEnumerator CatAct()
    {
        anim.SetTrigger("Open");
        door.SolvePuzzle();

        yield return new WaitForSeconds(2f); // 2초 기다림

        Unpossess();
        hasActivated = false;
    }
}
