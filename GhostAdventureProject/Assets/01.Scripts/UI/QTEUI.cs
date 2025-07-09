using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class QTEUI : MonoBehaviour
{
    public RectTransform needle;
    public float rotateSpeed = 90f;
    public KeyCode inputKey = KeyCode.Space;
    public Image successArc;
    public float minAngle;
    public float maxAngle;

    private float currentAngle = 0f;
    private bool isRunning = false;
    private bool goingBack = false; // 회전 방향
    private bool wasSuccess = false;

    public void ShowQTEUI()
    {
        currentAngle = 0f;
        needle.localEulerAngles = Vector3.zero;
        gameObject.SetActive(true);
        isRunning = true;
        goingBack = false;
        wasSuccess = false;
        ShowSuccessArc();
    }

    void Update()
    {
        if (!isRunning) return;
        
        float delta = rotateSpeed * Time.unscaledDeltaTime;
        // currentAngle += rotateSpeed * Time.unscaledDeltaTime;

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
                    Debug.Log("❌ QTE 실패 (입력 없음 또는 실패 범위)");
                    PossessionQTESystem.Instance.HandleQTEResult(false);
                }
                return;
            }
        }
        
        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);


        if (Input.GetKeyDown(inputKey))
        {

            if (currentAngle >= minAngle && currentAngle <= maxAngle)
            {
                Debug.Log("✅ QTE 성공!");
                wasSuccess = true;
                isRunning = false;
                gameObject.SetActive(false);
                PossessionQTESystem.Instance.HandleQTEResult(true);
            }
            else
            {
                Debug.Log("⛔ 입력했지만 실패 영역 (계속 회전)");
                wasSuccess = false;
                isRunning = false;
                gameObject.SetActive(false);
                PossessionQTESystem.Instance.HandleQTEResult(false);


            }
            // isRunning = false;
            // gameObject.SetActive(false);
            // bool success = (currentAngle >= minAngle && currentAngle <= maxAngle);
            // Debug.Log(success ? "✅ QTE 성공!" : "❌ QTE 실패 (타이밍 안 맞음)");
            // PossessionQTESystem.Instance.HandleQTEResult(success);
        }
    }

    void ShowSuccessArc()
    {
        minAngle = Random.Range(20, 160);
        maxAngle = minAngle + 15;
        float fill = (maxAngle - minAngle) / 360f;
        successArc.fillAmount = fill;
        successArc.rectTransform.localEulerAngles = new Vector3(0, 0, -minAngle);
    }
}
