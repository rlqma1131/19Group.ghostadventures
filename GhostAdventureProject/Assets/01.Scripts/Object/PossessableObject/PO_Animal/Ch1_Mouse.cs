using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Ch1_Mouse : MoveBasePossessable
{
    [Header("Mouse References")]
    [SerializeField] AudioClip mouse;
    [SerializeField] GameObject q_Key;

    [Header("팬 이벤트")]
    [SerializeField] Transform point1Transform;
    [SerializeField] Transform point2Transform;

    [Header("다용도실 쥐구멍")] 
    [SerializeField] Transform point3Transform;
    
    public bool canEnter;

    override protected void Start() {
        base.Start();
        spriteRenderer.flipX = true;
        hasActivated = false;

        Ch1_Mouse_Event relay = anim.GetComponent<Ch1_Mouse_Event>();
        if (relay != null)
            relay.mouse = this;

        Vector2 mousePos = transform.position;
        mousePos.y += 0.5f;
        q_Key.transform.position = mousePos;
    }

    override protected void Update() {
        if (!hasActivated) {
            q_Key.SetActive(false);
            return;
        }

        base.Update();
        if (isPossessed) InteractTutorial();

        if (canEnter)
            q_Key.SetActive(true);

        else if (!canEnter)
            q_Key.SetActive(false);

        if (Input.GetKeyDown(KeyCode.Q) && canEnter) {
            q_Key.SetActive(false);
            StartCoroutine(MouseAct());
        }
    }

    void LateUpdate() {
        highlightSpriteRenderer.flipX = spriteRenderer.flipX;
    }

    void InteractTutorial() {
        TutorialManager.Instance.Show(TutorialStep.Mouse_Possesse);
    }
    
    public void ActivateMouse() {
        spriteRenderer.flipX = false;
        anim.SetTrigger("Escape");

        // 1. 쥐구멍으로 도망
        transform.DOMove(point1Transform.position, 2.5f).SetEase(Ease.OutQuad);
    }

    //[이벤트함수] Escape 애니메이션의 마지막 프레임에 이벤트로 연결
    public void OnEscapeEnd() {
        transform.DOKill(); // 두트윈 종료

        // 2. 아이방으로 순간이동
        transform.position = point2Transform.position;
        anim.SetBool("Move", false);
        hasActivated = true;
        MarkActivatedChanged();
    }

    IEnumerator MouseAct() {
        // 쥐구멍 이동 (다용도실로)
        transform.position = point3Transform.position;

        // 1초 대기 후 빙의 해제
        yield return new WaitForSeconds(1f);

        hasActivated = false;
        MarkActivatedChanged();

        Unpossess();
        zoomCamera.Priority = 5;

        // 스르륵 사라지기
        spriteRenderer.DOFade(0f, 0.5f);
    }

    public override void OnPossessionEnterComplete() {
        base.OnPossessionEnterComplete();
        zoomCamera.Priority = 20;
        SoundManager.Instance.PlaySFX(mouse);
    }
}