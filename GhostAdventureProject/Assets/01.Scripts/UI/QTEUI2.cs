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
    public Image gaugeBar;              // 게이지바
    public TMP_Text timeText;           // 타임 텍스트
    public TextMeshProUGUI resultText;  // 결과 텍스트

    [Header("QTE Settings")]
    public AudioClip escape;            // 사운드
    public int requiredPresses = 15;    // 탈출 위해 필요한 키 입력 수
    public float timeLimit = 3f;        // 제한시간
    private int currentPressCount = 0;  // 현재 키 입력 수
    private float currentTime = 0f;     // 현재 시간
    private bool isRunning = false;     // 실행중인지 확인
    private bool isSuccess;             // 성공했는지 확인
    public bool isdead = false;         // 플레이어가 죽었는지 확인

    private CinemachineVirtualCamera camera;
    private CinemachineBasicMultiChannelPerlin noise;
    private float currentSize;          // 현재 확대 사이즈(카메라 효과)
    private float targetSize;           // 적 확대 사이즈(카메라 효과)

    public void Start()
    {
        resultText.gameObject.SetActive(false);
        qteUI.SetActive(false);
        isdead = false;
    }

    private void Update()
    {
        if (currentTime >= 2f)
        {
            timeText.color = Color.red; // 시간 초과시 빨간색으로 표시
        }
        else if (currentTime >= 1f)
        {
            timeText.color = Color.yellow; // 절반 시간은 노란색으로 표시
        }
        else
        {
            timeText.color = Color.white; // 정상 시간은 흰색으로 표시
        }
    }
    public void StartQTE()
    {
        if (isRunning) return;

        ResetState();
        if (qteUI) qteUI.SetActive(true);

        isRunning = true;

        // (카메라/참조 캐싱은 기존대로)
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
                resultText.text = "탈출 성공!";
                resultText.gameObject.SetActive(true);
                isSuccess = true;
                break;
            }
            // currentTime += Time.deltaTime;
            currentTime += Time.unscaledDeltaTime;
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
            resultText.text = "탈출 성공!";
            resultText.gameObject.SetActive(true);
            isSuccess = true;

            SoundManager.Instance.PlaySFX(escape);

            UIManager.Instance.PromptUI.ShowPrompt("다음에 잡히면 죽을거야... 잘 피해다니자", 2f);
        }
        
        // 탈출 실패시
        else
        {
            DOTween.To(() => camera.m_Lens.OrthographicSize, x => camera.m_Lens.OrthographicSize = x, currentSize, 0.3f);
            resultText.text = "탈출 실패!";
            resultText.gameObject.SetActive(true);
            isSuccess = false;
            isdead = true;
            UIManager.Instance.HideQTEEffectCanvas(); 
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.QTE_UI_2.gameObject.SetActive(false);
        }
        
        isRunning = false;
        yield return new WaitForSecondsRealtime(1.5f);
        qteUI.SetActive(false);
        resultText.gameObject.SetActive(false);
    }
    private IEnumerator ShakeCamera(float duration, float amplitude, float frequency)
    {
        noise.m_AmplitudeGain = amplitude;
        noise.m_FrequencyGain = frequency;

        yield return new WaitForSecondsRealtime(duration);

        noise.m_AmplitudeGain = 0f;
        noise.m_FrequencyGain = 0f;
    }
    
    public void ResetState()
    {
        StopAllCoroutines();

        // 순수 상태값 초기화
        currentPressCount = 0;
        currentTime = 0f;
        isRunning = false;
        isSuccess = false;
        isdead = false;

        // UI 초기화
        if (gaugeBar) gaugeBar.fillAmount = 0f;
        if (timeText) timeText.text = timeLimit.ToString("F2");
        if (resultText) { resultText.text = null; resultText.gameObject.SetActive(false); }
        if (qteUI)   qteUI.SetActive(false);

        // 카메라 연출 원복(혹시 남았을 수 있으니 안전하게)
        if (camera != null)
        {
            camera.m_Lens.OrthographicSize = currentSize;
            if (noise == null) noise = camera.GetCinemachineComponent<CinemachineBasicMultiChannelPerlin>();
            if (noise != null) { noise.m_AmplitudeGain = 0f; noise.m_FrequencyGain = 0f; }
        }
    }
    
    public bool IsQTERunning() => isRunning;
    public bool IsSuccess() => isSuccess;
}
