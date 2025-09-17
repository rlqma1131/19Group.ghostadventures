using UnityEngine;
using UnityEngine.UI;

public class Ch2_Doll_YameScan_fake : BaseInteractable
{
    [Header("Scan Settings")]
    [SerializeField] private float scan_duration = 2f; //스캔 시간
    [SerializeField] private GameObject scanPanel; //스캔 패널
    [SerializeField] private Image scanCircleUI; //스캔 원 UI
    
    [SerializeField] private GameObject player;
    // [SerializeField] private GameObject e_key;
    [SerializeField] private Ch2_Doll_YameScan_correct correctDoll; // 정답 인형

    // 내부 상태 변수
    private float scanTime;
    private bool isScanning;
    private bool isNear;
    // [SerializeField] private GameObject currentScanObject; // 현재 스캔 대상 오브젝트
    
    [SerializeField] private SoundEventConfig soundConfig;

    private Camera mainCamera;

    void Start()
    {
        mainCamera = Camera.main;
        if (player == null)
        {
            var pc = FindObjectOfType<PlayerController>();
            if (pc != null) player = pc.gameObject;
            if (player == null)
            {
                var pTag = GameObject.FindGameObjectWithTag("Player");
                if (pTag != null) player = pTag;
            }
        }

        // UI 자동 연결
        if (scanPanel == null || scanCircleUI == null)
        {
            if (UIManager.Instance != null && UIManager.Instance.scanUI != null)
            {
                scanPanel = UIManager.Instance.scanUI;
                var circleTr = scanPanel.transform.Find("CircleUI");
                if (circleTr != null) scanCircleUI = circleTr.GetComponent<Image>();
            }
        }

        // 시작 시 UI 꺼두기 (있을 때만)
        if (scanCircleUI != null) scanCircleUI.gameObject.SetActive(false);
        if (scanPanel != null) scanPanel.SetActive(false);
    }

    void Update()
    {
        if (correctDoll == null) return;

        // 스캔 가능한 상태가 아니거나, 스캔 중이 아닐 때 입력을 받음
        if (isNear && !isScanning && !correctDoll.clear_UnderGround && Input.GetKeyDown(KeyCode.E))
        {
            TryStartScan();
        }

        // 스캔이 진행 중일 때 로직 처리
        if (isScanning)
        {
            UpdateScan();
        }
    }

    void LateUpdate()
    {
        if (scanCircleUI != null && scanCircleUI.gameObject.activeInHierarchy && player != null && mainCamera != null)
        {
            scanCircleUI.transform.position =
                mainCamera.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
        }
    }

    private void TryStartScan()
    {
        // 영혼 에너지가 있는지 확인
        if (SoulEnergySystem.Instance != null && SoulEnergySystem.Instance.currentEnergy <= 0)
        {
            UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다", 2f);
            // 여기에 부족 알림 UI나 사운드를 재생하는 로직
            return;
        }

        StartScan();
    }

    private void StartScan()
    {
        isScanning = true;
        scanTime = 0f;

        UIManager.Instance?.PromptUI2?.ShowPrompt_UnPlayMode("스캔 시도 중...", 2f);

        if (scanPanel != null) scanPanel.SetActive(true);
        if (scanCircleUI != null)
        {
            scanCircleUI.gameObject.SetActive(true);
            scanCircleUI.fillAmount = 0f;
        }

        Time.timeScale = 0.3f;
        SoulEnergySystem.Instance?.Consume(1); // 없으면 무시
    }

    private void UpdateScan()
    {
        // 키를 계속 누르고 있는지 확인
        if (Input.GetKey(KeyCode.E))
        {
            scanTime += Time.unscaledDeltaTime;
            float t = Mathf.Clamp01(scanTime / scan_duration);
            if (scanCircleUI != null) scanCircleUI.fillAmount = t;

            if (scanTime >= scan_duration)
                CompleteScan();
        }
        // 키를 뗐을 경우 스캔 중단
        else
        {
            CancelScan("스캔이 중단");
        }
    }

    private void CompleteScan()
    {
        isNear = false;

        Time.timeScale = 1f;

        if (scanCircleUI != null) scanCircleUI.gameObject.SetActive(false);
        if (scanPanel != null) scanPanel.SetActive(false);

        // 사운드 트리거(옵션)
        if (soundConfig != null)
            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);

    }

    private void CancelScan(string reason)
    {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        if (scanCircleUI != null) scanCircleUI.gameObject.SetActive(false);
        if (scanPanel != null) scanPanel.SetActive(false);
    }

    protected override void OnTriggerEnter2D(Collider2D col)
    {
        base.OnTriggerEnter2D(col);
        if (col.CompareTag("Player") && correctDoll != null && !correctDoll.clear_UnderGround)
        {
            isNear = true;
            PlayerInteractSystem.Instance.AddInteractable(gameObject);  
        }
    }

    protected override void OnTriggerExit2D(Collider2D col)
    {
        base.OnTriggerExit2D(col);
        if (col.CompareTag("Player"))
        {
            isNear = false;
            if (isScanning) CancelScan("범위 이탈");
            PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        }
    }
}
