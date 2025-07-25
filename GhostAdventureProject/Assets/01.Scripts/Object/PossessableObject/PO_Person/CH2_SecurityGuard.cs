using System.Collections;
using UnityEngine;
using DG.Tweening;
using Unity.VisualScripting;

public enum GuardState { Idle, MovingToRadio, TurnOffRadio, MovingToBench, Resting, MovingToOffice, InOffice, Work, Roading }

public class CH2_SecurityGuard : MoveBasePossessable
{   
    // [SerializeField] private LockedDoor door; //도어락 있는 문 //없어도 될 것 같음
    [SerializeField] private Ch2_DoorLock doorLock; // 도어락
    [SerializeField] private SafeBox safeBox; // 금고
    [SerializeField] private Ch2_Radio radio; // 라디오
    public Transform Radio; // 라디오 위치
    public Transform bench; // 벤치 위치
    public Transform OfficeDoor_Outside; // 경비실 문(밖)
    public Transform OfficeDoor_Inside; // 경비실 문(안)
    public Transform chair; // 경비실 안 의자 위치
    private GuardState state; // 경비원의 상태
    private float turnOffRadioTimer = 0f;
    private float turnOffRadioDuration = 2f; // 라디오 끄는 시간
    private float restTimer = 0f; 
    public float restDuration = 3f; // 휴식시간
    private float roadingTimer = 0f;
    private float roadingDuration = 5f; // 로딩시간

    public Person targetPerson;
    public PersonConditionHandler conditionHandler;
    [SerializeField] private GameObject q_Key;
    private bool isNearDoor = false;
    private bool isInOffice;// 경비실 안에 있는지 확인
    private bool oneTimeShowClue = false; // 경비원 단서 - Clue:Missing 확대뷰어로 보여주기용(1번만)
    public bool isdoorLockOpen;
    private GameObject player;
    
    
    public bool doorPass = false;


    // 빙의되지 않았을 때 -> 라디오소리가 들리면 라디오를 따라감
    // 빙의가 풀렸을 때 -> 라디오 소리가 들려도 confused 상태가 됨


    
    // 처음 시작시 빙의불가(경비실안에 있음)
    protected override void Start()
    {
        base.Start();
        moveSpeed = 2f;
        hasActivated = false;
        isInOffice = true;
        targetPerson.currentCondition = PersonCondition.Unknown;
        player = FindObjectOfType<PlayerController>().gameObject;
    }

    protected override void Update()
    {
        if (radio != null && radio.IsPlaying)
        {
            // anim.Play("Idle");
            state = GuardState.MovingToRadio;
        }

        switch (state)
        {
            case GuardState.MovingToRadio:
                CheckInOut();
                break;

            case GuardState.TurnOffRadio:
                turnOffRadioTimer += Time.deltaTime;
                if(turnOffRadioTimer >= turnOffRadioDuration) 
                {
                    targetPerson.currentCondition = PersonCondition.Normal;
                    state = GuardState.MovingToBench;
                }
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
                MoveTo(OfficeDoor_Outside.position);
                break;
            
            case GuardState.InOffice:
                break;

            case GuardState.Work:
                MoveTo(chair.position);
                break;

            case GuardState.Roading:
                radio.triggerSound_Person.Stop();
                roadingTimer += Time.deltaTime;
                if(roadingTimer >= roadingDuration) 
                {
                    if(isInOffice)
                        state = GuardState.Work;
                    else
                        state = GuardState.MovingToOffice;
                }
                break;
        }

        if (!isPossessed)
            return;

        Move();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (doorPass)
            {
                OnDoorInteract();
                return;
            }
            zoomCamera.Priority = 5;
            Unpossess();
        }

        if (isPossessed)
        {
            // anim.Play("Idle");
        }
        // 단서 관련 로직 (추후 수정예정)---------------------------
        if (isPossessed && Input.GetKeyDown(KeyCode.Alpha7) && !oneTimeShowClue)
        {
            UIManager.Instance.InventoryExpandViewerUI.OnClueHidden += ShowText;
            oneTimeShowClue = true;
        }

        if (Input.GetKeyDown(KeyCode.Alpha1) || Input.GetKeyDown(KeyCode.Alpha2) || Input.GetKeyDown(KeyCode.Alpha3) || Input.GetKeyDown(KeyCode.Alpha4))
        {
            UIManager.Instance.InventoryExpandViewerUI.OnClueHidden -= ShowText;
        }
    }

    // 목적지까지 이동
    void MoveTo(Vector3 target)
    {   
        if(!isPossessed)
        {   
            anim.SetBool("Work", false);
            anim.SetBool("Rest", false);
            anim.SetBool("Move", true);
            Vector3 targetPos = transform.position;
            targetPos.x = target.x;
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            if(transform.position.x - target.x >0)
                spriteRenderer.flipX = true;
            else
                spriteRenderer.flipX = false;
            if (Mathf.Abs(transform.position.x - target.x) < 0.1f)
            {
                OnDestinationReached(target);
            }
        }
    }

    // 목적지 도착시 처리
    void OnDestinationReached(Vector3 destination)
    {
        // 라디오에 도착했을 때
        if (destination == Radio.position) 
        {
            state = GuardState.TurnOffRadio;
            anim.SetBool("Move", false);
        }

        // 벤치에 도착했을 때
        else if (destination == bench.position)
        {   
            state = GuardState.Resting;
            anim.SetBool("Move", false);
            anim.SetBool("Rest", true);
            restTimer = 0f;
        }
        
        // 경비실 문(밖)에 도착했을 때
        else if (destination == OfficeDoor_Outside.position)
        {
            if(!isPossessed)
            {
                Vector3 targetPos = transform.position;
                targetPos.x = OfficeDoor_Inside.position.x;
                transform.position = targetPos;
                state = GuardState.Work;
            }
        }

        // 경비실 의자에 도착했을 때
        else if (destination == chair.position)
        {
            anim.SetBool("Move", false);
            anim.SetBool("Work", true);
        }
    }

    // 경비실 문으로 이동 후 라디오로 이동 
    private void CheckInOut()
    {
        if(isInOffice)
        {
            MoveTo(OfficeDoor_Inside.position);
            if(transform.position.x == OfficeDoor_Inside.position.x)
            {   
                Vector3 guardPos = transform.position;
                guardPos.x = OfficeDoor_Outside.position.x + 0.5f;
                transform.position = guardPos;
            }

        }
        else
        {
            MoveTo(Radio.position);
        }
    }
    
    // QTE 관련 함수 (23일 이후 작업예정)  
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
    // 경비원이 있는 곳이 경비실 안인지 밖인지 확인 (트리거)
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);

        if(collision.CompareTag("In"))
        {
            isInOffice = true;
            hasActivated = false;
            targetPerson.currentCondition = PersonCondition.Unknown;
        }
    }
    // private void OnTriggerStay2D(Collider2D other)
    // {
    //     base.OnTriggerEnter2D(other);

    //     if(other.CompareTag("In"))
    //     {
    //         isInOffice = true;
    //         hasActivated = false;
    //         targetPerson.currentCondition = PersonCondition.Unknown;
    //     }
    // }
    
    
    
    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);

        if(collision.CompareTag("In"))
        {
            isInOffice = false;
            hasActivated = true;
        }
    }

    protected override void OnDoorInteract()
    {
        // 경비가 문을 통과함
        if(isPossessed && doorPass && isdoorLockOpen && !isInOffice)
        {
            Vector3 newPos = transform.position;
            newPos.x = OfficeDoor_Inside.position.x; // 또는 원하는 포인트
            transform.position = newPos;
        }
        else if(isPossessed && doorPass && isdoorLockOpen && isInOffice)
        {
            Vector3 newPos = transform.position;
            newPos.x = OfficeDoor_Outside.position.x; // 또는 원하는 포인트
            transform.position = newPos;
        }
    }

    // 빙의 해제시
    public override void Unpossess()
    {
        radio.triggerSound_Person.Stop();
        base.Unpossess();
        state = GuardState.Roading;
        anim.SetBool("Move", false);
        roadingTimer = 0f;
    }
    public override void OnPossessionEnterComplete()
    {
        base.OnPossessionEnterComplete();
    }

    void ShowText()
    {
        UIManager.Instance.PromptUI.ShowPrompt("잃어버린 게 뭘까...? 사람일까, 기억일까.", 2f);
    }
}