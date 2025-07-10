using DG.Tweening;
using UnityEngine;

public class Ch1_Mouse : MoveBasePossessable
{
    [SerializeField] private Transform point1Transform;
    [SerializeField] private Transform point2Transform;

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
        spriteRenderer.flipX = false;
        anim.SetTrigger("Escape");

        // 1. 쥐구멍으로 도망
        transform.DOMove(point1Transform.position, 2.5f).SetEase(Ease.OutQuad);
    }

    public void MouseCanObssessed()
    {
        hasActivated = true;
    }

    //[이벤트함수] Escape 애니메이션의 마지막 프레임에 이벤트로 연결
    public void OnEscapeEnd()
    {
        transform.DOKill(); // 두트윈 종료

        // 2. 아이방으로 순간이동
        transform.position = point2Transform.position;
        hasActivated = true;
        anim.SetBool("Move", false);
    }

    public void MouseAct()
    {
        // 쥐구멍 이동
        // 아이방 - 다용도실
    }
}
