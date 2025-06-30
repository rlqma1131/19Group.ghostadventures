using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class QTEUI2 : Singleton<QTEUI2>
{
    [Header("UI References")]
    public GameObject qteUI;
    public Image gaugeBar;
    public TMP_Text timeText;
    public TextMeshProUGUI success;
    public TextMeshProUGUI fail;
    // public TMP_Text resultText;

    [Header("QTE Settings")]
    public int requiredPresses = 15;
    public float timeLimit = 3f;

    private int currentPressCount = 0;
    private float currentTime = 0f;
    private bool isRunning = false;

    public void Start()
    {
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);
        qteUI.SetActive(false);
    }
    public void StartQTE()
    {
        qteUI.SetActive(true);
        currentPressCount = 0;
        currentTime = 0f;
        gaugeBar.fillAmount = 0f;
        // resultText.text = "";
        isRunning = true;
        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        while (currentTime < timeLimit)
        {
            currentTime += Time.deltaTime;
            float remainingTime = Mathf.Max(0f, timeLimit - currentTime);
            timeText.text = remainingTime.ToString("F2");

            if (Input.GetKeyDown(KeyCode.Space))
            {
                currentPressCount++;
                gaugeBar.fillAmount = Mathf.Clamp01((float)currentPressCount / requiredPresses);
            }

            yield return null;
        }

        isRunning = false;
        qteUI.SetActive(false);

        if (currentPressCount >= requiredPresses)
        {
            // resultText.text = "탈출 성공!";
            // resultText.color = Color.green;
            Debug.Log("탈출 성공");
            success.gameObject.SetActive(true);
            // yield return null;
            // 성공 처리 로직
        }
        else
        {
            // resultText.text = "탈출 실패!";
            // resultText.color = Color.red;
            Debug.Log("탈출 실패. GameOver");
            fail.gameObject.SetActive(true);
            // yield return null;
            // 실패 처리 로직
        }

        // resultText.gameObject.SetActive(true);
    }


    // (선택) 외부에서 QTE 상태 확인용
    public bool IsQTERunning() => isRunning;
}
