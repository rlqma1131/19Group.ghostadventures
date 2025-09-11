using System.Collections;
using System.Collections.Generic;
using System;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI : MonoBehaviour
{
    public RectTransform needle;            // 바늘
    public float rotateSpeed = 90f;         // 스피드(1초에 몇도를 돌지)
    public KeyCode inputKey = KeyCode.Space;
    public Image successArc;                // 성공영역
    public float minAngle;                  // 최소 각
    public float maxAngle;                  // 최대 각

    private float currentAngle = 0f;        // 현재 각
    private bool isRunning = false;         // 실행중인지 확인
    private bool goingBack = false;         // 회전 방향(false-시계방향/true-반시계방향)
    private bool wasSuccess = false;        // QTE 성공했는지 확인
    private Action<bool> resultCallback;

    // PossessionQTESystem에서 시작시 SetActive(false)됨
    public void ShowQTEUI()
    {
        ShowQTEUI(null);
    }
    
    public void ShowQTEUI(Action<bool> callback)
    {
        currentAngle = 0f;
        needle.localEulerAngles = Vector3.zero;
        isRunning = true;
        goingBack = false;
        wasSuccess = false;
        resultCallback = callback;
        ShowSuccessArc();
        gameObject.SetActive(true);
    }

    void Update()
    {
        if (!isRunning) return;
        
        float delta = rotateSpeed * Time.unscaledDeltaTime;

        if (!goingBack)
        {
            currentAngle += delta;
            if (currentAngle >= 180f)
            {
                currentAngle = 180f;
                goingBack = true; // 되돌아가기 시작
            }
        }
        else
        {
            currentAngle -= delta;
            if (currentAngle <= 0f)
            {
                currentAngle = 0f;
                isRunning = false;
                gameObject.SetActive(false);

                if (!wasSuccess)
                {
                    InvokeResult(false);
                }
                return;
            }
        }
        
        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);


        if (Input.GetKeyDown(inputKey))
        {
            wasSuccess = currentAngle >= minAngle && currentAngle <= maxAngle;
            isRunning = false;
            InvokeResult(wasSuccess);
        }
    }

    private void InvokeResult(bool result)
    {
        if(resultCallback != null)
            resultCallback.Invoke(result);
        else
            PossessionQTESystem.Instance.HandleQTEResult(result);
        
        gameObject.SetActive(false);
    }

    void ShowSuccessArc()
    {
        minAngle = UnityEngine.Random.Range(20, 150);
        maxAngle = minAngle + 30;
        float fill = (maxAngle - minAngle) / 360f;
        successArc.fillAmount = fill;
        successArc.rectTransform.localEulerAngles = new Vector3(0, 0, -minAngle);
    }
}
