using DG.Tweening;
using UnityEngine;

public class Ch1_Mouse : MoveBasePossessable
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private Vector2 point1; // 포인트1 좌표
    [SerializeField] private Vector2 point2; // 포인트2 좌표

    protected override void Start()
    {
        base.Start();

        spriteRenderer.flipX = true;
        hasActivated = false;

        var relay = anim.GetComponent<Ch1_Mouse_Event>();
        if (relay != null)
            relay.mouse = this;
    }

    protected override void Update()
    {
        if(!hasActivated)
            return;

        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        {
            MouseAct();
        }
    }

    public void ActivateMouse()
    {
        // 1. Move 애니메이션 활성화
        spriteRenderer.flipX = false; // 좌우 반전 해제
        anim.SetTrigger("Escape");

        // 2. point1까지 도망치는 이동 (1초)
        transform.DOMove(point1, 2.5f).SetEase(Ease.OutQuad).OnComplete(() =>
        {
            // 3. Escape 시작
            anim.SetTrigger("Escape");
        });
    }

    public void MouseCanObssessed()
    {
        hasActivated = true;
    }

    // 이 함수는 Escape 애니메이션의 마지막 프레임에 이벤트로 연결
    public void OnEscapeEnd()
    {
        // 4. point2로 순간이동
        transform.position = point2;
        hasActivated = true; // 빙의 가능 상태로 변경
        anim.SetBool("Move", false); // Idle 상태로 대기
    }

    public void MouseAct()
    {
        // 쥐구멍 이동
        // 아이방 - 다용도실
    }
}
