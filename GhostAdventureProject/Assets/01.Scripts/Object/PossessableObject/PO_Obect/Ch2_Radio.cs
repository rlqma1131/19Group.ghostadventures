using Cinemachine;
using System.Collections;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class Ch2_Radio : BasePossessable
{   
    //콜라이더 트리거에 닿으면
    //하이라이트 빛나고
    //E키 눌러서 빙의 가능
    //Q키를 누르면 줌 됨 -> Ch2_RadioControll 스크립트로 넘어감

    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private GameObject Q_Key;

    private Collider2D col;
    private bool isControlMode = false;
    private int channel = 1;

    protected override void Start()
    {
        hasActivated = false;

        col = GetComponent<Collider2D>();
    }

    public void ActivateTV()
    {
        if (hasActivated) return;

        hasActivated = true;
        if (anim != null)
            anim.SetTrigger("On");
    }

    protected override void Update()
    {
        if (!isPossessed || !hasActivated)
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
        
        // 채널 변경
        if (Input.GetKeyDown(KeyCode.W)) 
        { 
            channel = Mathf.Clamp(channel + 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            // zoomAnim.SetTrigger("Change");
        }
        if (Input.GetKeyDown(KeyCode.S)) 
        { 
            channel = Mathf.Clamp(channel - 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            // zoomAnim.SetTrigger("Change");
        }

        // 확정은 Space
        if (channel == 9 && Input.GetKeyDown(KeyCode.Space))
        {
            // 정답 효과음 추가

            ShowMemoryandDoorOpen();
            hasActivated = false;
            col.enabled = false;

        }
    }

    private void UpdateChannelDisplay()
    {
        // if(channelTxt != null)
        //     channelTxt.text = ($"0{channel}");
    }

    private void ShowMemoryandDoorOpen()
    {
        // 1. TV 줌 애니메이션 재생
        // show.SetActive(true);
        anim.SetTrigger("Solved");
        // zoomAnim.SetTrigger("Show");
        StartCoroutine(WaitZoomEnding(3f));
    }

    private IEnumerator WaitZoomEnding(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 3. 문 열기

        // 4. 빙의 해제
        isPossessed = false;
        isControlMode = false;
        UIManager.Instance.PlayModeUI_OpenAll();
        zoomCamera.Priority = 5;
        Unpossess();
    }

    public override void OnPossessionEnterComplete() 
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        zoomCamera.Priority = 20; // 빙의 시 카메라 우선순위 높이기
        isControlMode = true;
        isPossessed = true;
        // channelTxt.text = "01"; // 초기 채널 표시
        UpdateChannelDisplay();
    }
}
