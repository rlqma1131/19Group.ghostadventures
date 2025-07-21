using System.Collections;
using UnityEngine;
using DG.Tweening;

public enum GuardState { Idle, MovingToRadio, MovingToBench, Resting, MovingToOffice, InOffice }

public class CH2_SecurityGuard : MoveBasePossessable
{   
    [SerializeField] private LockedDoor door; //도어락 있는 문 //없어도 될 것 같음
    [SerializeField] private SafeBox safeBox; // 금고
    [SerializeField] private Ch2_Radio radio; // 라디오
    public Transform Radio;
    public Transform bench;
    public Transform office;

    private GuardState state; 
    private float restTimer = 0f;
    public float restDuration = 3f;
    public Person targetPerson;
    public PersonConditionHandler conditionHandler;
    
    [SerializeField] private GameObject q_Key;
    private bool isNearDoor = false;
    
    protected override void Start()
    {
        base.Start();
        hasActivated = true;
        moveSpeed = 8f;
    }

    protected override void Update()
    {
        if (!hasActivated) return;

        if (isNearDoor)
        {
            // Vector2 catPos = this.transform.position;
            // catPos.y += 0.5f;
            // q_Key.transform.position = catPos;
            // q_Key.SetActive(true);
        }
        else if (!isNearDoor)
        {
            // q_Key.SetActive(false);
        }

        base.Update();

        if (Input.GetKeyDown(KeyCode.Q) && isNearDoor)
        {
        }

        if(radio != null && radio.IsPlaying)
        {
            state = GuardState.MovingToRadio;
        }

        switch (state)
        {
            case GuardState.MovingToRadio:
                MoveTo(Radio.position);
                break;

            case GuardState.MovingToBench:
                MoveTo(bench.position);
                break;

            case GuardState.Resting:
                restTimer += Time.deltaTime;
                if (restTimer >= restDuration)
                {
                    targetPerson.currentCondition = PersonCondition.Vital;
                    state = GuardState.MovingToOffice;
                }
                break;

            case GuardState.MovingToOffice:
                MoveTo(office.position);
                break;
            
            case GuardState.InOffice:
                hasActivated = false;
                break;
        }
    }

    // 목적지까지 이동
    void MoveTo(Vector3 target)
    {   
        Vector3 targetPos = transform.position;
        targetPos.x = target.x;
        transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

        if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
        {
            OnDestinationReached(target);
        }
    }

    // 목적지 도착시 처리
    void OnDestinationReached(Vector3 destination)
    {
        if (destination == Radio.position)
        {
            targetPerson.currentCondition = PersonCondition.Normal;
            state = GuardState.MovingToBench;
        }
        else if (destination == bench.position)
        {
            targetPerson.currentCondition = PersonCondition.Normal;
            state = GuardState.Resting;
            restTimer = 0f;
        }
        else if (destination == office.position)
        {
            state = GuardState.Idle;
            targetPerson.currentCondition = PersonCondition.Normal;
            Debug.Log("경비실 도착. 상태 초기화");
        }
    }

private void StopRadioSound()
{
    Debug.Log("라디오소리 그만!");
}
    //트리거 감지 예시
public void OnRadioTriggered()
{
    if (state == GuardState.Idle || state == GuardState.MovingToOffice)
    {
        targetPerson.currentCondition = PersonCondition.Tired;
        state = GuardState.MovingToRadio;
        Debug.Log("라디오 소리 탐지! 라디오로 이동");
    }
}

     public void SetCondition(PersonCondition condition)
    {
        targetPerson.currentCondition = condition;
        switch (condition)
        {
            case PersonCondition.Vital:
                conditionHandler = new VitalConditionHandler();
                break;
            case PersonCondition.Normal:
                conditionHandler = new NormalConditionHandler();
                break;
            case PersonCondition.Tired:
                conditionHandler = new TiredConditionHandler();
                break;
        }
    }

    public override void OnQTESuccess()
    {
        SoulEnergySystem.Instance.RestoreAll();

        PossessionStateManager.Instance.StartPossessionTransition();
    }

    // 문 근처에 있는지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if (collision.GetComponent<LockedDoor>() == door)
        {
            isNearDoor = true;
        }

        // bool doorlockopen = collision.GetComponent<DoorLock>().doorOpen;
        // if(doorlockopen)
        // {
        //     StartCoroutine(SecurityGuardAct());
        // }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

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
        float jumpHeight = 1.5f;
        float jumpDuration = 0.4f;

        // 현재 위치 저장
        Vector3 originalPos = transform.position;

        // 2. 점프 시퀀스
        DG.Tweening.Sequence jumpSequence = DOTween.Sequence();

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

    IEnumerator SecurityGuardAct()
    {
        anim.SetTrigger("Open");
        // door.SolvePuzzle();

        yield return new WaitForSeconds(2f); // 2초 기다림

        zoomCamera.Priority = 1;
        Unpossess();
        anim.Play("Cat_Sleeping");
        hasActivated = false;
    }
}
