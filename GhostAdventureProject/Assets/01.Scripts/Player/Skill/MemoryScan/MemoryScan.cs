using _01.Scripts.Extensions;
using _01.Scripts.Player;
using UnityEngine;
using UnityEngine.UI;
using static _01.Scripts.Utilities.Timer;

public class MemoryScan : MonoBehaviour
{
    [Header("Scan Settings")] 
    [SerializeField] float scan_duration = 4f; //스캔 시간

    [Header("UI References")] 
    [SerializeField] GameObject scanPanel; //스캔 패널
    [SerializeField] Image scanCircleUI; //스캔 원 UI
    [SerializeField] Transform playerTransform; //플레이어 위치

    [Header("Scanned Object References")]
    [SerializeField] GameObject currentScanObject; // 현재 스캔 대상 오브젝트
    [SerializeField] MemoryFragment currentMemoryFragment;
    [SerializeField] AudioClip scanSound;

    CountdownTimer scanTimer;
    Camera cam;
    Inventory_Player inventory_Player;
    Player player;

    // 내부 상태 변수
    float scanTime;
    bool isScanning;
    bool isNearMemory;
    bool isScanKeydown;
    
    // 기억조각 스캔 가능 여부
    bool isScannable;
    
    public MemoryFragment CurrentMemoryFragment => currentMemoryFragment;

    public void Initialize(Player player) {
        // Reference 설정
        inventory_Player = UIManager.Instance.Inventory_PlayerUI.GetComponent<Inventory_Player>();
        cam = Camera.main;
        this.player = player;
        
        // 초기 UI 상태 설정
        scanPanel = UIManager.Instance.scanUI;
        scanCircleUI = scanPanel?.GetComponentInChildren_SearchByName<Image>("CircleUI", true);
        scanCircleUI?.gameObject.SetActive(false);

        // Timer 초기 설정
        scanTimer = new CountdownTimer(scan_duration);
        scanTimer.OnTimerStart += StartScan;
        scanTimer.OnTimerStop += () => {
            if (scanTimer.IsFinished) CompleteScan();
            else {
                SoundManager.Instance.StopSFX();
                CancelScan("스캔 키 입력 중단");
            }
        };
    }

    void Update() {
        if (player.InteractSystem.CurrentClosest != currentScanObject) return;

        isScannable = currentMemoryFragment && currentMemoryFragment.IsScannable();
        if (!isScannable) return;

        isScanKeydown = Input.GetKey(KeyCode.E);
        
        // 스캔 가능한 상태가 아니거나, 스캔 중이 아닐 때 입력을 받음
        if (isNearMemory && !isScanning && isScanKeydown) {
            UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("스캔 시도 중...", 2f);
            TryStartScan();
        }
        
        // Tick timer
        scanTimer.Tick(Time.unscaledDeltaTime);
        
        // 스캔이 진행 중일 때 로직 처리
        if (isScanning) {
            UpdateScan();
        }
    }

    void LateUpdate() {
        // UI가 활성화 되어 있을 때만 플레이어 위치를 따라다니도록 처리
        if (scanCircleUI != null && scanCircleUI.gameObject.activeInHierarchy) {
            // 플레이어의 월드 위치를 스크린 위치로 변환하여 UI 위치를 갱신
            scanCircleUI.transform.position =
                cam.WorldToScreenPoint(playerTransform.position) + new Vector3(-40, 50, 0);
        }
    }

    void TryStartScan() {
        // 영혼 에너지가 있는지 확인
        if (player.SoulEnergy.CurrentEnergy <= 0) {
            UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
            return;
        }

        scanTimer.Start();
    }

    void StartScan() {
        if (!currentMemoryFragment.IsScannable()) { scanTimer.Stop(); return; }
        
        isScanning = true;
        scanPanel?.SetActive(true);
        
        if (scanCircleUI) {
            scanCircleUI.gameObject.SetActive(true);
            scanCircleUI.fillAmount = 0f;
        }
        else {
            Debug.LogWarning("Scan Circle UI가 설정되지 않았습니다. UI를 확인해주세요.");
            return;
        }

        Time.timeScale = 0.3f; // 슬로우 모션 시작
        player.SoulEnergy.Consume(1); // 에너지 소모
        Debug.Log("스캔 시작");

        SoundManager.Instance.PlaySFX(scanSound);
    }

    void UpdateScan() {
        // 키를 계속 누르고 있는지 확인
        if (isScanKeydown) scanCircleUI.fillAmount = 1f - scanTimer.Progress;
        else scanTimer.Stop();
    }

    void CompleteScan() {
        Debug.Log("스캔 완료");
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);

        // 이미 스캔되었는지 확인
        if (currentMemoryFragment != null && currentMemoryFragment.IsScannable()) {
            currentMemoryFragment.IsScannedCheck();
        }
        else {
            Debug.Log("이미 스캔된 기억 조각이거나, MemoryFragment 컴포넌트가 없습니다.");
        }

        currentScanObject.GetComponentInChildren<SpriteRenderer>().color =
            new Color(155 / 255f, 155 / 255f, 155 / 255f); // 스캔 완료 후 색상 변경
    }

    void CancelScan(string reason) {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
    }

    void OnTriggerEnter2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Memory")) {
            isNearMemory = true;
            currentScanObject = collision.gameObject;
            currentMemoryFragment = currentScanObject.GetComponent<MemoryFragment>();
            Debug.Log($"기억 조각과 접촉: {currentMemoryFragment?.data.memoryID}");
        }
    }

    void OnTriggerExit2D(Collider2D collision) {
        if (collision.gameObject.layer == LayerMask.NameToLayer("Memory")) {
            isNearMemory = false;

            // 스캔 중에 범위를 벗어났다면 스캔을 취소
            if (isScanning) {
                CancelScan("범위를 이탈하여 스캔이 중단되었습니다.");
            }
            
            currentScanObject = null;
            currentMemoryFragment = null;

            Debug.Log("기억 조각과 접촉 해제");
        }
    }
}