using DG.Tweening;
using UnityEngine;

public class Ch1_Mouse : MoveBasePossessable
{
    [SerializeField] private Vector2 point1 = new Vector2(-3.5f, -1.5f); // 포인트1 좌표
    [SerializeField] private Vector2 point2 = new Vector2(3.5f, -1.5f); // 포인트2 좌표

    protected override void Start()
    {
        base.Start();

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
        anim.SetBool("Move", true);

        // 2. point1까지 도망치는 이동 (1초)
        transform.DOMove(point1, 1f).SetEase(Ease.OutQuad).OnComplete(() =>
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
        anim.SetBool("Move", false); // Idle 상태로 대기
    }

    public void MouseAct()
    {
        // 기타 고유 기능
    }
}
