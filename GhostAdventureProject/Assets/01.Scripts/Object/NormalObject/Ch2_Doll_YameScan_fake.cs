using UnityEngine;
using UnityEngine.UI;

public class Ch2_Doll_YameScan_fake : MemoryFragment
{
    [SerializeField] private float scan_duration = 2f;  //스캔 시간
    [SerializeField] private GameObject scanPanel;      //스캔 패널
    [SerializeField] private Image scanCircleUI;        //스캔 원 UI
    [SerializeField] private Ch2_Doll_YameScan_correct correctDoll; // 정답 인형
    [SerializeField] private SoundEventConfig soundConfig;     

    private float scanTime;
    private bool isScanning;
    private bool isNearMemory;
    
    protected override void Start(){
        base.Start();
        isScannable = true;

        scanPanel = UIManager.Instance.scanUI;
        scanCircleUI = UIManager.Instance.scanUI.transform.Find("CircleUI")?.GetComponent<Image>();
        scanCircleUI?.gameObject.SetActive(false);
    }

    void Update()
    {
        if (correctDoll == null) return;

        if (isNearMemory && isScannable && !isScanning && Input.GetKeyDown(KeyCode.E)) {
            TryStartScan();
        }

        if (isScanning) {
            UpdateScan();
        }

        // 지하수로 문이 열렸다면 스캔불가능하게 변경
        if (correctDoll.isOpen_UnderGroundDoor && isScannable) {
            isScannable = false;
        }
       
    }

    void LateUpdate() {
        if (scanCircleUI != null && scanCircleUI.gameObject.activeInHierarchy) {
            scanCircleUI.transform.position =
                Camera.main.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
        }
    }

    void TryStartScan(){
        // 영혼 에너지가 있는지 확인
        if (player.SoulEnergy.CurrentEnergy <= 0) {
            UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
            return;
        }

        StartScan();
    }

    void StartScan() {
        isScanning = true;
        scanTime = 0f;
        UIManager.Instance.PromptUI2.ShowPrompt2_UnPlayMode("스캔 시도 중...");
        scanPanel?.SetActive(true);
        if (scanCircleUI != null) {
            scanCircleUI.gameObject.SetActive(true);
            scanCircleUI.fillAmount = 0f;
        }
        else {
            Debug.LogWarning("Scan Circle UI가 설정되지 않았습니다. UI를 확인해주세요.");
            return;
        }
        Time.timeScale = 0.3f; // 슬로우 모션 시작
        player.SoulEnergy.Consume(1); // 에너지 소모
    }

    void UpdateScan(){
        if (Input.GetKey(KeyCode.E)) {
            scanTime += Time.unscaledDeltaTime;
            float scanProgress = Mathf.Clamp01(scanTime / scan_duration);
            scanCircleUI.fillAmount = scanProgress;

            if (scanTime >= scan_duration) {
                CompleteScan();
            }
        }
        else {
            CancleScan("스캔이 중단");
        }
    }

    void CompleteScan()
    {
        isNearMemory = false;
        isScannable = false;
        isScanning = false;

        Time.timeScale = 1f;

        if (scanCircleUI != null) scanCircleUI.gameObject.SetActive(false);
        if (scanPanel != null) scanPanel.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();

        // 사운드 트리거(옵션)
        if (soundConfig != null)
            SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);

    }

    void CancleScan(string reason) {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();
    }

    protected override void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && isScannable) {
            isNearMemory = true;
            player.InteractSystem.AddInteractable(gameObject);  
        }
    }

    protected override void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            isNearMemory = false;
            player.InteractSystem.RemoveInteractable(gameObject);
            // 스캔 중에 범위를 벗어났다면 스캔을 취소
            if (isScanning) {
                CancleScan("범위를 이탈하여 스캔이 중단되었습니다.");
            }
        }
    }
}
