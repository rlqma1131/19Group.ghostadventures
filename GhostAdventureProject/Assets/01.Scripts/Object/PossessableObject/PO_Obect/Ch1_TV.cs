using Cinemachine;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ch1_TV : BasePossessable
{
    [SerializeField] private CinemachineVirtualCamera zoomCamera;
    [SerializeField] private Animator zoomAnim;
    [SerializeField] private GameObject memoryObject;
    [SerializeField] private GameObject show;
    [SerializeField] private TextMeshProUGUI channelTxt;
    [SerializeField] private LockedDoor Door;

    private bool isControlMode = false;
    private int channel = 1;

    protected override void Start()
    {
        show.SetActive(false);
        hasActivated = false;
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
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isControlMode)
            {
                // 조작 중이면 조작 종료
                isControlMode = false;
                zoomCamera.Priority = 5;
            }
            else
            {
                // 조작 시작
                isControlMode = true;
                zoomCamera.Priority = 20; // 카메라 우선순위 높이기
            }
        }
        
        if (!isControlMode && Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
        }

        if (!isControlMode) return;

        // 채널 변경
        if (Input.GetKeyDown(KeyCode.W)) 
        { 
            channel = Mathf.Clamp(channel + 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }
        if (Input.GetKeyDown(KeyCode.S)) 
        { 
            channel = Mathf.Clamp(channel - 1, 1, 9); 
            UpdateChannelDisplay();
            // 채널 변경 효과음 추가
            zoomAnim.SetTrigger("Change");
        }

        // 확정은 Space
        if (channel == 9 && Input.GetKeyDown(KeyCode.Space))
        {
            // 정답 효과음 추가
            ShowMemoryandDoorOpen();
            hasActivated = false;
        }
    }

    private void UpdateChannelDisplay()
    {
        if(channelTxt != null)
            channelTxt.text = ($"0{channel}");
    }

    private void ShowMemoryandDoorOpen()
    {
        // 1. TV 줌 애니메이션 재생
        show.SetActive(true);
        zoomAnim.SetTrigger("Show");
        StartCoroutine(WaitZoomEnding(3f));
    }

    private IEnumerator WaitZoomEnding(float delay)
    {
        yield return new WaitForSeconds(delay);

        // 2. 기억조각 보이기
        if (memoryObject != null)
        {
            memoryObject.SetActive(true);
            anim.SetTrigger("Solved");
        }

        // 3. 문 열기
        Door.SolvePuzzle();

        isControlMode = false;
        zoomCamera.Priority = 5;
        Unpossess();
    }
}
