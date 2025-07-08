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
    public float minAngle = 120f;
    public float maxAngle = 150f;

    private float currentAngle = 0f;
    private bool isRunning = false;

    public void ShowQTEUI()
    {
        currentAngle = 0f;
        needle.localEulerAngles = Vector3.zero;
        gameObject.SetActive(true);
        isRunning = true;
        ShowSuccessArc();
    }

    void Update()
    {
        if (!isRunning) return;
        currentAngle += rotateSpeed * Time.unscaledDeltaTime;

        if (currentAngle >= 180f)
        {
            isRunning = false;
            gameObject.SetActive(false);
            Debug.Log("❌ QTE 실패");
            PossessionQTESystem.Instance.HandleQTEResult(false);
            return;
        }

        needle.localEulerAngles = new Vector3(0, 0, -currentAngle);

        if (Input.GetKeyDown(inputKey))
        {
            isRunning = false;
            gameObject.SetActive(false);
            bool success = (currentAngle >= minAngle && currentAngle <= maxAngle);
            Debug.Log(success ? "✅ QTE 성공!" : "❌ QTE 실패 (타이밍 안 맞음)");
            PossessionQTESystem.Instance.HandleQTEResult(success);
        }
    }

    void ShowSuccessArc()
    {
        float fill = (maxAngle - minAngle) / 360f;
        successArc.fillAmount = fill;
        successArc.rectTransform.localEulerAngles = new Vector3(0, 0, -minAngle);
    }
}
