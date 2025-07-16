using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_LeftSwing : BasePossessable
{
    private int qteSuccessCount = 0;
    private int totalQTECount = 3;
    private bool isQTESequenceRunning = false;
    private QTEUI qteUI;

    protected override void Start()
    {
        base.Start();
        qteUI = FindObjectOfType<QTEUI>();
    }
    
    protected override void Update()
    {
        base.Update();

        if (!isPossessed || isQTESequenceRunning)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("Q 키 입력 - QTE 시퀀스 시작");
            isQTESequenceRunning = true;
            qteSuccessCount = 0;
            StartNextQTE();
        }
    }
    
    private void StartNextQTE()
    {
        Time.timeScale = 0.3f;
        qteUI.ShowQTEUI(OnQTEResult);
    }

    private void OnQTEResult(bool success)
    {
        Time.timeScale = 1f;

        if (!success)
        {
            Debug.Log("QTE 실패 - 빙의 해제");
            isQTESequenceRunning = false;
            Unpossess();
            return;
        }

        qteSuccessCount++;
        Debug.Log($"QTE 성공 {qteSuccessCount}/{totalQTECount}");

        if (qteSuccessCount >= totalQTECount)
        {
            Debug.Log("QTE 3회 성공 - 전원 ON");
            hasActivated = true;
            isQTESequenceRunning = false;
        }
        else
        {
            StartNextQTE();
        }
    }
}
