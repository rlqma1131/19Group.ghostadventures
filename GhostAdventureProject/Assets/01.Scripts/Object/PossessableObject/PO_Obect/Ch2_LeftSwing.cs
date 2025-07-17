using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_LeftSwing : BasePossessable
{
    [SerializeField] private Ch2_Kiosk targetKiosk;
    [SerializeField] private Ch2_Computer targetComputer;
    [SerializeField] private GameObject q_Key;
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
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            q_Key.SetActive(false);
            isQTESequenceRunning = true;
            qteSuccessCount = 0;
            StartNextQTE();
        }
        q_Key.SetActive(true);
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
            isQTESequenceRunning = false;
            Unpossess();
            return;
        }

        qteSuccessCount++;
        Debug.Log($"QTE 성공 {qteSuccessCount}/{totalQTECount}");

        if (qteSuccessCount >= totalQTECount)
        {
            anim.SetTrigger("LeftSwing");
            // 컴퓨터, 키오스크 hasActivated = true 포함된 매서드 추가
            if (targetKiosk != null)
                targetKiosk.Activate();
            if(targetComputer != null)
                targetComputer.Activate();
            
            Unpossess();
            isQTESequenceRunning = false;
            hasActivated = false;
        }
        else
        {
            StartNextQTE();
        }
    }
}
