using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_Radio : BasePossessable
{  
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject zoomRadio; // 라디오(줌)
    [SerializeField] private GameObject needle; // 주파수 바늘
    [SerializeField] private float range = 0.0324f; // 주파수 바늘 이동범위
    [SerializeField] private float triggerX_Person = 0.38f; // 트리거위치 - 사람
    [SerializeField] private float triggerX_Enemy = 0.09f; // 트리거위치 - 적
    public AudioClip triggerSound_Person; // 트리거사운드 - 사람
    public AudioClip triggerSound_Enemy; // 트리거사운드 - 적
    public bool hasTriggered_Person = false; // 트리거발동여부 - 사람
    private bool hasTriggered_Enemy = false; // 트리거발동여부 - 적
    private bool isControlMode = false; // 주파수 조정가능 모드(줌)
    [SerializeField] private Animator speakerOn; // 스피커 애니메이션 재생용
    [SerializeField] private GameObject musicalNoteOn; // 음표 애니메이션 재생용
    [SerializeField] private CH2_SecurityGuard guard;
    [SerializeField] private SoundEventConfig soundConfig;
    
    public bool isPlaying = false;

    protected override void Start()
    {
        base.Start();
        zoomRadio.SetActive(false);
    }

    protected override void Update()
    {
        if(guard.UseAllItem)
            hasActivated = false;

        if(isPlaying)
        {
            speakerOn.SetBool("OnSpeaker", true); // 스피커 애니메이션 재생
            musicalNoteOn.SetActive(true);
            Animator musicalNoteAni = musicalNoteOn.GetComponent<Animator>();
            musicalNoteAni.SetBool("OnSpeaker", true);
        }
        else if(!isPlaying)
        {
            speakerOn.SetBool("OnSpeaker", false); // 스피커 애니메이션 재생
            Animator musicalNoteAni = musicalNoteOn.GetComponent<Animator>();
            musicalNoteAni.SetBool("OnSpeaker", false);
            musicalNoteOn.SetActive(false);
        }

        if (!isPossessed)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (isControlMode)
            {
                // 조작 종료
                isControlMode = false;
                isPossessed = false;
                UIManager.Instance.PlayModeUI_OpenAll();
                zoomRadio.SetActive(false);
                zoomCamera.Priority = 5;
                Unpossess();
            }
        }
        // 주파수 변경
        if (Input.GetKeyDown(KeyCode.A))
        {
            if(isControlMode)
                GoToLeft();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            if(isControlMode)
                GoToRight();
        }
        // 사람 ===========================================================================================
        // needle의 위치가 트리거 위치에 가까워지면 사운드 작동.
        if (!hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) <= 0.01f)
        {
            SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드 아웃
            SoundManager.Instance.FadeInLoopingSFX(triggerSound_Person, 1f, 0.5f);
            hasTriggered_Person = true;
            isPlaying = true;
            guard.targetPerson.SetCondition(PersonCondition.Tired);
            Debug.Log(guard.conditionHandler);
            zoomRadio.SetActive(false);
            zoomCamera.Priority = 5;
            UIManager.Instance.PlayModeUI_OpenAll();
            Unpossess();
        }
        // needle의 위치가 트리거 위치에 멀어지면 사운드 중지.
        if (hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) >= 0.01f)
        {
            SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
            SoundManager.Instance.RestoreLastBGM(1f);
            hasTriggered_Person = false;
            isPlaying = false;
        }

        // 적 ==============================================================================================
        if (!hasTriggered_Enemy && Mathf.Abs(needle.transform.localPosition.x - triggerX_Enemy) <= 0.01f)
        {
            SoundManager.Instance.FadeOutAndStopBGM(1f); // BGM 페이드 아웃
            SoundManager.Instance.FadeInLoopingSFX(triggerSound_Enemy, 1f, 0.5f);
            isPlaying = true;
            hasTriggered_Enemy = true;
            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
            Debug.Log(guard.conditionHandler);
            zoomRadio.SetActive(false);
            zoomCamera.Priority = 5;
            UIManager.Instance.PlayModeUI_OpenAll();
            Unpossess();
            
            
            // AttractPerson();
            UIManager.Instance.PromptUI.ShowPrompt("음악이 나왔어. 누군가 반응할지도 몰라.");
            
        }
        // needle의 위치가 트리거 위치에 멀어지면 사운드 중지.
        if (hasTriggered_Enemy && Mathf.Abs(needle.transform.localPosition.x - triggerX_Enemy) >= 0.01f)
        {
            SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
            SoundManager.Instance.RestoreLastBGM(1f);
            isPlaying = false;
            hasTriggered_Enemy = false;
        }
        // ===============================================================================================
    }

    private void GoToLeft()
    {   
        Vector3 needlePos = needle.transform.localPosition;
        Debug.Log("Needle X Pos: " + needle.transform.position.x);
        needlePos.x -= range;
        needlePos.x = Mathf.Max(needlePos.x, 0.08815f);
        needle.transform.localPosition = needlePos;
    }
    private void GoToRight()
    {
        Vector3 needlePos = needle.transform.localPosition;
        Debug.Log("Needle X Pos: " + needle.transform.position.x);
        needlePos.x += range;
        needlePos.x = Mathf.Min(needlePos.x, 0.51f);
        needle.transform.localPosition = needlePos;
    }

    // 사람을 끌어들임
    private void AttractPerson()
    {
        StartCoroutine(WaitZoomEnding(2f));
        speakerOn.SetBool("OnSpeaker", true); // 스피커 애니메이션 재생
    }

    // 적을 끌어들임
    private void AttractEnemy()
    {

    }

    private IEnumerator WaitZoomEnding(float delay)
    {
        yield return new WaitForSeconds(delay);
        // 스피커 애니메이션 재생됨

        // 4. 빙의 해제
        isPossessed = false;
        isControlMode = false;
        UIManager.Instance.PlayModeUI_OpenAll();
        zoomCamera.Priority = 5;
        zoomRadio.SetActive(false);
        Unpossess();
    }

    public override void OnPossessionEnterComplete() 
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomRadio.SetActive(true);
        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Person"))
        {
            SoundManager.Instance.FadeOutAndStopLoopingSFX(1f);
            SoundManager.Instance.RestoreLastBGM(1f);
            isPlaying = false;
            GoToLeft();
        }        
    }
    
}
