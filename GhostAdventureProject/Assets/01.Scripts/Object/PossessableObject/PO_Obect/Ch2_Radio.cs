using Cinemachine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Ch2_Radio : BasePossessable
{  
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject needle; // 주파수 바늘
    [SerializeField] private float range = 0.0324f; // 주파수 바늘 이동범위
    [SerializeField] private float triggerX_Person = 0.38f; // 트리거위치 - 사람
    [SerializeField] private float triggerX_Enemy; // 트리거위치 - 적
    [SerializeField] private AudioSource triggerSound_Person; // 트리거사운드 - 사람
    [SerializeField] private AudioSource EnemyTriggerSound_Enemy; // 트리거사운드 - 적
    private bool hasTriggered_Person = false; // 트리거발동 - 사람
    private bool hasTriggered_Enemy = false; // 트리거발동 - 적
    private bool isControlMode = false; // 주파수조정 가능모드(줌)
    [SerializeField] private Animator speakerOn; // 스피커 애니메이션 재생용
    [SerializeField] private GameObject UICanvas; // UI
    [SerializeField] private CH2_SecurityGuard guard;
    public bool IsPlaying=> triggerSound_Person.isPlaying;


    protected override void Start()
    {
        hasActivated = true;
        UICanvas.SetActive(false);
    }

    protected override void Update()
    {
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

        // needle의 위치가 트리거 위치에 가까워지면 사운드 작동.
        if (!hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) <= 0.01f)
        {
            triggerSound_Person.Play();
            hasTriggered_Person = true;
            guard.SetCondition(PersonCondition.Tired);
            Debug.Log(guard.conditionHandler);
            // AttractPerson();
        }
        // needle의 위치가 트리거 위치에 멀어지면 사운드 중지.
        if (hasTriggered_Person && Mathf.Abs(needle.transform.localPosition.x - triggerX_Person) >= 0.01f)
        {
            triggerSound_Person.Stop();
            hasTriggered_Person = false;
        }

        if(IsPlaying)
        {
            speakerOn.SetBool("OnSpeaker", true); // 스피커 애니메이션 재생
        }
        else if(!IsPlaying)
        {
            speakerOn.SetBool("OnSpeaker", false); // 스피커 애니메이션 재생
        }
            
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
        UICanvas.SetActive(false);
        zoomCamera.Priority = 5;
        Unpossess();
    }

    public override void OnPossessionEnterComplete() 
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        UICanvas.SetActive(true);
        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Person"))
        {
            triggerSound_Person.Stop();
        }        
    }
}
