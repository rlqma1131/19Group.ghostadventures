using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ch1_TV : BasePossessable
{
    [SerializeField] private Camera zoomCamera;
    [SerializeField] private GameObject memoryObject;
    [SerializeField] private GameObject doorOpen; // 열릴 문
    [SerializeField] private TextMeshProUGUI channelTxt;

    private bool isControlMode = false;
    private int channel = 1;
    
    [SerializeField] private Animator TvAnimator;
    [SerializeField] private Animator DoorAnimator;

    protected override void Start()
    {
        hasActivated = false;
    }

    public void ActivateTV()
    {
        if (hasActivated) return;

        hasActivated = true;
        if (TvAnimator != null)
            TvAnimator.SetTrigger("PowerOn");
        channelTxt.gameObject.SetActive(true);

        Debug.Log("TV 전원 켜짐 - 이제 빙의 가능");
    }

    protected override void Update()
    {
        // if (!isPossessed || !hasActivated )
        //     return;
        if (!isPossessed )
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (isControlMode)
            {
                // 조작 중이면 조작 종료
                isControlMode = false;
                zoomCamera.gameObject.SetActive(false);
            }
            else
            {
                // 조작 시작
                isControlMode = true;
                zoomCamera.gameObject.SetActive(true);
            }
        }
        
        if (!isControlMode && Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
        }

        if (!isControlMode) return;

        if (Input.GetKeyDown(KeyCode.W)) { channel = Mathf.Clamp(channel + 1, 1, 9); UpdateChannelDisplay(); }
        if (Input.GetKeyDown(KeyCode.S)) { channel = Mathf.Clamp(channel - 1, 1, 9); UpdateChannelDisplay(); }


        if (channel == 9 && Input.GetKeyDown(KeyCode.A))
        {
            ShowMemoryandDoorOpen();
            hasActivated = false;
        }
    }

    private void UpdateChannelDisplay()
    {
        if(channelTxt != null)
            channelTxt.text = channel.ToString();
    }

    private void ShowMemoryandDoorOpen()
    {
        Debug.Log("채널 9에서 A 입력 → 연출 시작");

        isControlMode = false;
        zoomCamera.gameObject.SetActive(false);

        // 1. 연출 오브젝트 보이기
        if (memoryObject != null)
            memoryObject.SetActive(true);

        // 2. 문 열기
        if (doorOpen != null)
        {
            if (DoorAnimator != null)
                DoorAnimator.SetTrigger("DoorOpen");
            Debug.Log("문 열림 완료");
        }
    }
}
