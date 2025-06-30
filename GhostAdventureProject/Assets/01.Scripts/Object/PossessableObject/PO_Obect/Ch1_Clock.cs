using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Clock : BasePossessable
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Camera zoomCamera;
    [SerializeField] private Ch1_TV  tvObject;
    
    private bool isControlMode = false;

    private int hour = 0;
    private int minute = 0;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed) return;

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
        
        if(!isControlMode) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            hour = (hour + 1) % 12;
            UpdateHands();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            minute = (minute + 1) % 60;
            UpdateHands();
        }

        if (hour == 8 && minute == 14)
        {
            Debug.Log("정답");
            tvObject.ActivateTV();
            isControlMode = false;
            zoomCamera.gameObject.SetActive(false);
            hasActivated = true;
        }
    }

    private void UpdateHands()
    {
        if(hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -30f * hour);
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -6f * minute);
    }
}
