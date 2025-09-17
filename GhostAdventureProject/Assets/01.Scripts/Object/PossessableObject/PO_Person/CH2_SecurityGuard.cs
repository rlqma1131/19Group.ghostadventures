using UnityEngine;
using DG.Tweening;
using System.Linq;

public enum GuardState 
{ 
    MovingToRadio, 
    TurnOffRadio, 
    MovingToBench, 
    Resting, 
    MovingToOffice, 
    Work, 
    Roading // 빙의 해제했을 때
}

public class CH2_SecurityGuard : MoveBasePossessable
{   
    [SerializeField] private Ch2_DoorLock doorLock; // 도어락
    [SerializeField] private Ch2_SafeBox safeBox;   // 금고
    [SerializeField] private Ch2_Radio radio;       // 라디오
    [SerializeField] private GameObject q_Key;

    [Header("이동위치")]
    public Transform Radio;                 // 라디오 
    public Transform bench;                 // 벤치 
    public Transform OfficeDoor_Outside;    // 경비실 문(밖)
    public Transform OfficeDoor_Inside;     // 경비실 문(안)
    public Transform chair;                 // 경비실 안 의자

    private float turnOffRadioTimer = 0f;   // 라디오 끄는 시간 계산용 타이머
    private float turnOffRadioDuration = 2f; // 라디오 끄는 시간
    private float restTimer = 0f;           // 휴식시간 계산용 타이머
    public float restDuration = 3f;         // 휴식시간
    private float roadingTimer = 0f;        // 로딩시간 계산용 타이머 (빙의해제 후)
    private float roadingDuration = 5f;     // 로딩시간

    private GuardState state;               
    public PersonConditionUI conditionUI; 
    private HaveItem haveitem;              

    private bool isInOffice;                // 경비원이 경비실 안에 있는지 확인
    private bool oneTimeShowClue = false;   // 경비원 단서 - Clue:Missing 확대뷰어로 보여주기용(1번만) //코드 수정하기 
    public bool isdoorLockOpen;             // 도어락이 열렸는지 확인 (도어락 스크립트에서 정보 넣어줌)
    public bool doorPass = false;           // 문 앞에 있는지 확인
    private BoxCollider2D[] cols;           // 콜라이더 (경비실 의자에 앉았을 때 콜라이더 없앰)

    protected override void Start()
    {
        base.Start();
        moveSpeed = 2f;
        isInOffice = true;
        state = GuardState.Work;
        haveitem = GetComponent<HaveItem>();
        conditionUI = GetComponent<PersonConditionUI>();
        conditionUI.currentCondition = PersonCondition.Tired;
        cols = GetComponentsInChildren<BoxCollider2D>();
    }

    protected override void Update()
    {   
        // 라디오가 재생되고 있을 때
        if (radio != null && radio.IsPlaying)
        {
            conditionUI.currentCondition = PersonCondition.Tired;
            state = GuardState.MovingToRadio;
        }
        
        switch (state)
        {
            case GuardState.MovingToRadio:
                CheckInOut_GoToRadio();
                break;

            case GuardState.TurnOffRadio:
                turnOffRadioTimer += Time.deltaTime;
                if(turnOffRadioTimer >= turnOffRadioDuration) 
                {
                    conditionUI.currentCondition = PersonCondition.Normal;
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
                    conditionUI.currentCondition = PersonCondition.Vital;
                    TutorialManager.Instance.Show(TutorialStep.SecurityGuard_AfterRest);
                    state = GuardState.MovingToOffice;
                }
                break;

            case GuardState.MovingToOffice:
                MoveTo(OfficeDoor_Outside.position);
                break;

            case GuardState.Work:
                MoveTo(chair.position);
                break;

            case GuardState.Roading:
                conditionUI.currentCondition = PersonCondition.Normal;
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

        if(radio.IsPlaying)
        {
            // 빙의했을 때 라디오가 켜져있다면 점차 소리 줄어듬
            radio.triggerSound_Person.DOFade(0f, 1.5f)
            .OnComplete(() => radio.triggerSound_Person.Stop());
        }
        UIManager.Instance.tabkeyUI.SetActive(true);

        Move();

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (doorPass)
            {
                OnDoorInteract();
                return;
            }

            if (haveitem.IsInventoryEmpty())
            {
                zoomCamera.Priority = 5;
                Unpossess();
                hasActivated = false;
                MarkActivatedChanged();
            }
            else
                UIManager.Instance.PromptUI.ShowPrompt("뭔가 더 얻을 수 있는게 있을것 같아");
        }

        if (!haveitem.IsHasItem("MISSING") && !oneTimeShowClue)
        {
            UIManager.Instance.InventoryExpandViewerUI.OnClueHidden += ShowText;
            oneTimeShowClue = true;
            SaveManager.MarkPuzzleSolved("메모3");
        }
    }

    // 목적지(target)까지 이동
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
            anim.SetBool("Rest", true);
            restTimer = 0f;
        }
        
        // 경비실 문(밖)에 도착했을 때
        else if (destination == OfficeDoor_Outside.position)
        {
            if(!isPossessed)
            {
                TutorialManager.Instance.Show(TutorialStep.SecurityGuard_InOffice);
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
            conditionUI.currentCondition = PersonCondition.Vital;
            
            if(!hasActivated)  DisabledCollider();
        }
    }
    
    // 콜라이더 없앰
    private void DisabledCollider()
    {
        foreach(BoxCollider2D col in cols)
        {
            col.enabled = false;
        }
    }

    // 경비원이 있는 곳이 안인지 밖인지 확인 후 라디오로 가게만듬.
    private void CheckInOut_GoToRadio()
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
            TutorialManager.Instance.Show(TutorialStep.SecurityGuard_GoToRadio);
        }
    }

    // 경비원이 있는 곳이 경비실 안인지 밖인지 확인
    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        base.OnTriggerEnter2D(collision);
        if(collision.CompareTag("In"))
        {
            isInOffice = true;
            MarkActivatedChanged();
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision)
    {
        base.OnTriggerExit2D(collision);
        if(collision.CompareTag("In"))
        {
            isInOffice = false;
            MarkActivatedChanged();
        }
    }

    // 문 앞에서 E키 눌렀을 때 빙의해제되는게 아니고 다른 문으로 이동
    protected override void OnDoorInteract()
    {
        if(isPossessed && doorPass && isdoorLockOpen)
        {
            if(!isInOffice)
            {
                Vector3 newPos = transform.position;
                newPos.x = OfficeDoor_Inside.position.x;
                transform.position = newPos;
            }
            else
            {
                Vector3 newPos = transform.position;
                newPos.x = OfficeDoor_Outside.position.x;
                transform.position = newPos;
            }
        }
    }

    // 빙의 해제시 상태=로딩
    public override void Unpossess()
    {
        base.Unpossess();
        UIManager.Instance.tabkeyUI.SetActive(false);
        state = GuardState.Roading;
        anim.SetBool("Move", false);
        roadingTimer = 0f;
    }

    // 빙의 성공시 작동하던 애니메이션 중지
    public override void OnPossessionEnterComplete() 
    {   
        base.OnPossessionEnterComplete();
        anim.SetBool("Rest", false);
        anim.SetBool("Move", false); 
    }

    // 단서 획득시 대사 출력
    void ShowText()
    {
        UIManager.Instance.PromptUI.ShowPrompt("잃어버린 게 뭘까...? 사람일까, 기억일까.", 2f);
    }
}