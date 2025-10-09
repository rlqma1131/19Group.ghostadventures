using System.Collections;
using DG.Tweening;
using UnityEngine;

public class Ch1_Cat : MoveBasePossessable
{
    // Animation String hash
    readonly static int IdleHash = Animator.StringToHash("Idle");
    readonly static int SleepHash = Animator.StringToHash("Sleep");
    readonly static int OpenHash = Animator.StringToHash("Open");
    
    [SerializeField] LockedDoor door;
    [SerializeField] GameObject q_Key;
    [SerializeField] Animator highlightAnim;
    [SerializeField] AudioClip catMeow;

    bool isNearDoor;
    bool isActing;

    override protected void Start() {
        base.Start();
        hasActivated = false;
    }

    override protected void Update() {
        if (!hasActivated) {
            q_Key.SetActive(false);
            return;
        }

        switch (isNearDoor) {
            case true:
            {
                Vector2 catPos = transform.position;
                catPos.y += 0.5f;
                q_Key.transform.position = catPos;
                q_Key.SetActive(true);
                break;
            }
            case false: q_Key.SetActive(false); break;
        }

        if (!isPossessed || !player.PossessionSystem.CanMove) return;

        if (isActing) return;

        Move();

        if (Input.GetKeyDown(KeyCode.E)) {
            zoomCamera.Priority = 5;
            Unpossess();
            anim.SetBool(MoveHash, false);
        }

        if (Input.GetKeyDown(KeyCode.Q) && isNearDoor) {
            q_Key.SetActive(false);
            // 문 열기
            StartCoroutine(CatAct());
        }
    }

    void LateUpdate() => highlightSpriteRenderer.flipX = spriteRenderer.flipX;

    public override void OnQTESuccess() {
        player.SoulEnergy.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    override protected void OnTriggerEnter2D(Collider2D collision) {
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<LockedDoor>() == door) {
            isNearDoor = true;
        }

        if (collision.CompareTag("Player") && !SaveManager.IsPuzzleSolved("후라이팬")) {
            UIManager.Instance.PromptUI.ShowPrompt("잠들어 있어..깨워볼까?");
        }
    }

    override protected void OnTriggerExit2D(Collider2D collision) {
        base.OnTriggerExit2D(collision);

        if (collision.GetComponent<LockedDoor>() == door) {
            isNearDoor = false;
        }
    }

    public void Blink() {
        anim.SetTrigger("Blink");
    }

    public void ActivateCat() {
        SoundManager.Instance.PlaySFX(catMeow);
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
            MarkActivatedChanged();
            anim.SetBool(IdleHash, true);
        });
    }

    IEnumerator CatAct() {
        isActing = true;

        anim.SetTrigger(OpenHash);
        door.UnlockDoors();

        yield return new WaitForSecondsRealtime(2f); // 2초 기다림

        zoomCamera.Priority = 5;
        hasActivated = false;
        Unpossess();

        yield return new WaitForSecondsRealtime(0.5f);

        anim.SetBool(MoveHash, false);
        anim.SetBool(IdleHash, false);
        anim.SetTrigger(SleepHash);
        MarkActivatedChanged();
    }
}