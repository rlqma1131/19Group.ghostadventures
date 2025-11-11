using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_Radio : BasePossessable
{  
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject zoomRadio;      // 라디오(줌)
    [SerializeField] private GameObject needle;         // 주파수 바늘
    [SerializeField] private float range = 0.0324f;     // 주파수 바늘 이동범위
    [SerializeField] private float triggerX_Person = 0.38f; // 트리거위치 - 사람 
    [SerializeField] private float triggerX_Enemy = 0.09f;  // 트리거위치 - 적
    public AudioSource triggerSound_Person;             // 트리거사운드 - 사람
    public AudioSource triggerSound_Enemy;              // 트리거사운드 - 적
    private bool hasTriggered_Person = false;           // 트리거발동여부 - 사람
    private bool hasTriggered_Enemy = false;            // 트리거발동여부 - 적
    private bool isControlMode = false;                 // 주파수 조정가능 모드(줌)인지 확인
    [SerializeField] private Animator speakerOn;        // 스피커 애니메이션 재생용
    [SerializeField] private GameObject musicalNoteOn;  // 음표 오브젝트
    [SerializeField] private Animator musicalNoteAni;   // 음표 애니메이션 재생용
    [SerializeField] private Ch2_SecurityGuard guard;
    public bool IsPlaying=> triggerSound_Person.isPlaying; // 트리거사운드 - 사람 재생중인지 확인
    [SerializeField] private SoundEventConfig soundConfig;

    // 저장 확인용 불값 ( 저장 후 중복 저장 방지 )
    private bool isSaved = false;

    protected override void Start()
    {
        base.Start();

        zoomRadio.SetActive(false);
        musicalNoteAni = musicalNoteOn.GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (isSaved)
            return;

        if (!guard.HasActivated())
        {
            hasActivated = false;
            MarkActivatedChanged();
            isSaved = true;
        }

        // 트리거사운드-사람 이 재생되고 있다면 스피커 작동
        if (IsPlaying)  SpeakerOn(true);
        else            SpeakerOn(false);


        if (!isPossessed) return;

        if (isControlMode)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                ExitControlMode();
                Unpossess();
            }

            // 주파수 변경
            if (Input.GetKeyDown(KeyCode.A))
                GoToLeft();
            else if (Input.GetKeyDown(KeyCode.D))
                GoToRight();
        }
        
        // 사람 ===========================================================================================
        // 주파수 바늘의 위치가 triggerX_Person 위치에 가까워지면 트리거사운드 작동.
        if (!hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) <= 0.01f)
        {
            triggerSound_Person.Play();
            hasTriggered_Person = true;
            ExitControlMode();
            Unpossess();
        }
        // 주파수 바늘의 위치가 triggerX_Person 위치에서 멀어지면 사운드 중지.
        if (hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) >= 0.01f)
        {
            triggerSound_Person.Stop();
            hasTriggered_Person = false;
        }

        // 적 ==============================================================================================
        // 주파수 바늘의 위치가 triggerX_Enemy 위치에 가까워지면 트리거사운드 작동.
        if (!hasTriggered_Enemy && Mathf.Abs(needle.transform.localPosition.x - triggerX_Enemy) <= 0.01f)
        {
            // triggerSound_Enemy.Play();
            hasTriggered_Enemy = true;
            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
            ExitControlMode();
            Unpossess();
            UIManager.Instance.PromptUI.ShowPrompt("음악이 나왔어. 누군가 반응할지도 몰라.");
        }
        // 주파수 바늘의 위치가 triggerX_Enemy 위치에서 멀어지면 사운드 중지.
        if (hasTriggered_Enemy && Mathf.Abs(needle.transform.localPosition.x - triggerX_Enemy) >= 0.01f)
        {
            // triggerSound_Enemy.Stop();
            hasTriggered_Enemy = false;
        }
        // ===============================================================================================
    }

    // 주파수 바늘을 왼쪽으로 옮김
    private void GoToLeft()
    {   
        Vector3 needlePos = needle.transform.localPosition;
        needlePos.x -= range;
        needlePos.x = Mathf.Max(needlePos.x, 0.08815f);
        needle.transform.localPosition = needlePos;
    }

    // 주파수 바늘을 오른쪽으로 옮김
    private void GoToRight()
    {
        Vector3 needlePos = needle.transform.localPosition;
        needlePos.x += range;
        needlePos.x = Mathf.Min(needlePos.x, 0.51f);
        needle.transform.localPosition = needlePos;
    }

    // 빙의 완료시 컨트롤모드(라디오(줌)) 진입
    public override void OnPossessionEnterComplete() 
    {
        base.OnPossessionEnterComplete();
        
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomRadio.SetActive(true);
        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;
    }

    // 컨트롤 모드(라디오(줌))에서 빠져나감
    private void ExitControlMode()
    {
        UIManager.Instance.PlayModeUI_OpenAll();
        zoomRadio.SetActive(false);
        zoomCamera.Priority = 5;
        isControlMode = false;
        isPossessed = false;
    }

    // 스피커 작동
    private void SpeakerOn(bool value)
    {
        speakerOn.SetBool("OnSpeaker", value);       // 스피커 애니메이션 재생
        musicalNoteOn.SetActive(value);              // 음표 오브젝트 표시
        musicalNoteAni.SetBool("OnSpeaker", value);  // 음표 애니메이션 재생
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Person"))
        {
            // 경비원과 닿으면 노래 중지
            triggerSound_Person.Stop();
            GoToLeft();
        }        
    }
}
