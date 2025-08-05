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
    public bool isdead = false;

    private CinemachineVirtualCamera camera;
    private float currentSize;
    private float targetSize;

    private CinemachineBasicMultiChannelPerlin noise;
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
        noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
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
                StartCoroutine(ShakeCamera(0.3f, 3f, 10f));
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
            isdead = true;
        }
        
        isRunning = false;
        yield return new WaitForSeconds(1.5f);
        qteUI.SetActive(false);
        success.gameObject.SetActive(false);
        fail.gameObject.SetActive(false);

    }
    private IEnumerator ShakeCamera(float duration, float amplitude, float frequency)
    {
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;

        yield return new WaitForSeconds(duration);

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }
    // (선택) 외부에서 QTE 상태 확인용
    public bool IsQTERunning() => isRunning;
    public bool IsSuccess() => isSuccess;
}
