using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_TV : BasePossessable
{
    [SerializeField] private Camera zoomCamera;
    [SerializeField] private GameObject memoryObject;
    [SerializeField] private GameObject doorOpen; // 열릴 문

    private bool isControlMode = false;
    private int channel = 1;
    
    [SerializeField] private Animator animator;
    
    public void ActivateTV()
    {
        if (isCompleted) return;

        isCompleted = true;
        if (animator != null)
            animator.SetTrigger("PowerOn");

        Debug.Log("TV 전원 켜짐 → 이제 빙의 가능");
    }

    protected override void Update()
    {
        base.Update();
        
        if (!isPossessed || !isCompleted )
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            isControlMode = !isControlMode;
            zoomCamera.gameObject.SetActive(isControlMode);
        }

        if (!isControlMode) return;

        if (Input.GetKeyDown(KeyCode.W)) { channel = Mathf.Clamp(channel + 1, 1, 9); }
        if (Input.GetKeyDown(KeyCode.S)) { channel = Mathf.Clamp(channel - 1, 1, 9); }

        if (channel == 9 && Input.GetKeyDown(KeyCode.A))
        {
            ShowMemoryandDoorOpen();
        }
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
            if (animator != null)
                animator.SetTrigger("DoorOpen");
            Debug.Log("문 열림 완료");
        }
    }
}
