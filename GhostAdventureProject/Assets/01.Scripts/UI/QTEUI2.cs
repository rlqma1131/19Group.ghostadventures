using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using Cinemachine;
using DG.Tweening;
public class QTEUI2 : MonoBehaviour
{
    [Header("UI References")]
    public GameObject qteUI;
    public Image gaugeBar;
    public TMP_Text timeText;
    public TextMeshProUGUI success;
    public TextMeshProUGUI fail;

    [Header("QTE Settings")]
    public int requiredPresses = 15;
    public float timeLimit = 3f;
    private int currentPressCount = 0;
    private float currentTime = 0f;
    private bool isRunning = false;
    private bool isSuccess;

    private CinemachineVirtualCamera camera;
    private float currentSize;
    private float targetSize;


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
        isRunning = true;
        camera = GameManager.Instance.Player.GetComponent<PlayerCamera>().currentCam;
        currentSize = camera.m_Lens.OrthographicSize;
        StartCoroutine(RunQTE());
    }

    private IEnumerator RunQTE()
    {
        
        while (currentTime < timeLimit)
        {

           
            if (currentPressCount >= requiredPresses)
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
                 
                 
                 targetSize = camera.m_Lens.OrthographicSize - currentSize * 0.05f;
                DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, targetSize, 0.3f);
                currentPressCount++;
                gaugeBar.fillAmount = Mathf.Clamp01((float)currentPressCount / requiredPresses);
            }

            yield return null;
        }
       

        // 탈출 성공시
        if (currentPressCount >= requiredPresses)
        {
            DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, currentSize, 0.3f);
            success.gameObject.SetActive(true);
            isSuccess = true;
        }
        
        // 탈출 실패시
        else
        {
            DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, currentSize, 0.3f);
            fail.gameObject.SetActive(true);
            isSuccess = false;
        }
        
        isRunning = false;
        yield return new WaitForSeconds(1.5f);
        qteUI.SetActive(false);
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);

    }

    // (선택) 외부에서 QTE 상태 확인용
    public bool IsQTERunning() => isRunning;
    public bool IsSuccess() => isSuccess;
}
