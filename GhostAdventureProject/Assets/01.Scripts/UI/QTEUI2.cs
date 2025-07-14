using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
//using UnityEditor.AnimatedValues;

public class QTEUI2 : MonoBehaviour
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
    private bool isSuccess;
    private bool canStartQTE = true;

    public void Start()
    {
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);
        qteUI.SetActive(false);
    }
    public void StartQTE()
    {
        // if(!canStartQTE || isRunning)
        //     return;
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
            if(currentPressCount >= requiredPresses)
            {
                success.gameObject.SetActive(true);
                isSuccess = true;
                break;
            }
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

        
        // 탈출 성공시
        if (currentPressCount >= requiredPresses)
        {
            // resultText.text = "탈출 성공!";
            // resultText.color = Color.green;
            success.gameObject.SetActive(true);
            // yield return null;
            // 성공 처리 로직
            isSuccess = true;
            // StartCoroutine(QTECooldown(2f));

        }
        // 탈출 실패시
        else
        {
            // resultText.text = "탈출 실패!";
            // resultText.color = Color.red;
            fail.gameObject.SetActive(true);
            isSuccess = false;
            // yield return null;
            // 실패 처리 로직
        }
        isRunning = false;
        yield return new WaitForSeconds(1.5f);
        qteUI.SetActive(false);
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);

        // resultText.gameObject.SetActive(true);
    }

    // private IEnumerator QTECooldown(float delay)
    // {
    //     canStartQTE = false;
    //     yield return new WaitForSeconds(delay);
    //     canStartQTE = true;
    // }


    // (선택) 외부에서 QTE 상태 확인용
    public bool IsQTERunning() => isRunning;
    public bool IsSuccess() => isSuccess;
}
