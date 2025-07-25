using System.Collections;
using UnityEngine;
using DG.Tweening;

public class Ch2_Raven : MoveBasePossessable
{

    [SerializeField] private LockedDoor underGroundDoor; // 지하창고 연결 문
    [SerializeField] private Animator highlightAnim;
    [SerializeField] private GameObject SandCastle;

    private bool isNearDoor = false;
    private bool sandCastleBreakAble = false;

        [SerializeField] private float flyForce = 5f;
    [SerializeField] private float diveSpeed = 5f;

    protected override void Start()
    {
        base.Start();
        hasActivated = true;
    }

    protected override void Update()
    {
        if (!hasActivated)
        {
            return;
        }
        if (isNearDoor || sandCastleBreakAble)
        {
            Vector2 catPos = this.transform.position;
            catPos.y += 0.5f;
            // q_Key.SetActive(true);
        }
        else if (!isNearDoor)
        {
        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.Q))
        { 
            if(isPossessed)
            {
                anim.SetTrigger("Attack");
            }
    
            if(isNearDoor)
            {
                // underGroundDoor.SolvePuzzle();
                // 문 열기
                // StartCoroutine(CatAct());
            }
            // if(sandCastleBreakAble)
            // {
            //     // anim.SetBool("Attack", true);
            //     underGroundDoor.SolvePuzzle();
            //     Debug.Log("문이 열렸습니다");
            // }
        }
        // HandleFlight();
    }

    protected override void Move()
    {
        float h = Input.GetAxis("Horizontal");
        float v = Input.GetAxis("Vertical");
        Vector3 move = new Vector3(h, v, 0);
        // 이동 여부 판단
        bool isMoving = move.sqrMagnitude > 0.01f;

        if (anim != null)
        {
            anim.SetBool("Move", isMoving);
        }
        if (isMoving)
        {
            transform.position += move * moveSpeed * Time.deltaTime;

            // 좌우 Flip
            if (spriteRenderer != null && Mathf.Abs(h) > 0.01f)
            {
                spriteRenderer.flipX = h < 0f;
            }
        }
    }
        
    // void HandleFlight()
    // {
    // if(isPossessed)
    // {
    // // 날기: 스페이스바
    // if (Input.GetKey(KeyCode.Space))
    // {
    //     transform.position += Vector3.up * flyForce * Time.deltaTime;
    //     // if (anim != null)
    //         // anim.SetBool("Fly", true);
    // }
    // else
    // {
    //     // if (anim != null)
    //         // anim.SetBool("Fly", false);
    // }

    // 급강하: S 키
    // if (Input.GetKey(KeyCode.S))
    // {
    //     transform.position += Vector3.down * diveSpeed * Time.deltaTime;
    //     // if (anim != null)
    //     //     anim.SetTrigger("Dive"); // 애니메이션이 있다면
    //     }
    // }
    // }


    public override void OnQTESuccess()
    {
        SoulEnergySystem.Instance.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<LockedDoor>() == underGroundDoor)
        {
            isNearDoor = true;
        }

        if(collision.gameObject == SandCastle)
        {
            Debug.Log("모래성파괴가능범위진입");
            sandCastleBreakAble = true;
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if (collision.GetComponent<LockedDoor>() == underGroundDoor)
        {
            isNearDoor = false;
        }
        if(collision.gameObject == SandCastle)
        {
            Debug.Log("모래성파괴가능범위_탈출");
            sandCastleBreakAble = false;
        }
    }

    public void Blink()
    {
        // anim.SetTrigger("Blink");
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
            // anim.SetBool("Idle", true);
        });
    }

    // IEnumerator CatAct()
    // {
    //     anim.SetTrigger("Open");
    //     underGroundDoor.SolvePuzzle();

    //     yield return new WaitForSeconds(2f); // 2초 기다림

    //     zoomCamera.Priority = 5;
    //     Unpossess();
    //     anim.Play("Cat_Sleeping");
    //     hasActivated = false;
    // }
}
