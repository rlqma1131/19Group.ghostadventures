using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class YameScan_correct : BaseInteractable
{
    [Header("Scan Settings")] 
    [SerializeField] float scan_duration = 2f; // 스캔 시간
    [SerializeField] GameObject scanPanel; // 스캔 패널
    [SerializeField] Image scanCircleUI; // 스캔 원 UI
    [SerializeField] GameObject playerObj;
    [SerializeField] GameObject door; // 지하수로와 연결된 문
    [SerializeField] GameObject shelf; // 문 막고 있는 책장
    [SerializeField] GameObject clue_P; // 단서 P
    [SerializeField] List<ClueData> clueitems; // 소모되는 단서 아이템
    public bool clear_UnderGround; // 지하수로 문이 열렸는지 확인

    [Header("Current Scanned Object")] [SerializeField]
    GameObject currentScanObject; // 현재 스캔 대상 오브젝트

    // 내부 상태 변수
    float scanTime;
    bool isScanning;
    bool isNearMemory;

    // Cam Ref.
    Camera mainCamera;

    override protected void Start() {
        base.Start();
        mainCamera = Camera.main;

        // 초기 UI 상태 설정
        scanPanel = UIManager.Instance.scanUI;
        scanCircleUI = UIManager.Instance.scanUI.transform.Find("CircleUI")?.GetComponent<Image>();
        scanCircleUI?.gameObject.SetActive(false);
        playerObj = player.gameObject;
        clue_P.SetActive(false);
    }

    void Update() {
        // 스캔 가능한 상태가 아니거나, 스캔 중이 아닐 때 입력을 받음
        if (isNearMemory && !isScanning && Input.GetKeyDown(KeyCode.E)) {
            TryStartScan();
        }

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
                mainCamera.WorldToScreenPoint(playerObj.transform.position) + new Vector3(-40, 50, 0);
        }
    }

    void TryStartScan() {
        // 영혼 에너지가 있는지 확인
        if (player.SoulEnergy.CurrentEnergy <= 0) {
            UIManager.Instance.PromptUI.ShowPrompt("에너지가 부족합니다");
            // 여기에 부족 알림 UI나 사운드를 재생하는 로직
            return;
        }

        StartScan();
    }

    void StartScan() {

        isScanning = true;
        scanTime = 0f;

        UIManager.Instance.PromptUI2.ShowPrompt_UnPlayMode("스캔 시도 중...", 3f);

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
        Debug.Log("스캔 시작");
    }

    void UpdateScan() {
        // 키를 계속 누르고 있는지 확인
        if (Input.GetKey(KeyCode.E)) {
            scanTime += Time.unscaledDeltaTime; // Time.timeScale에 영향받지 않는 시간으로 진행
            float scanProgress = Mathf.Clamp01(scanTime / scan_duration);
            scanCircleUI.fillAmount = scanProgress;

            // 스캔 완료 체크
            if (scanTime >= scan_duration) {
                CompleteScan();
            }
        }
        // 키를 뗐을 경우 스캔 중단
        else {
            CancleScan("스캔이 중단");
        }
    }

    void CompleteScan() {
        Debug.Log("스캔 완료");
        isNearMemory = false;
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);

        door.SetActive(true);
        // Animator doorani = door.GetComponent<Animator>();
        // doorani.SetBool("Open", true);
        Vector3 shelfPos = shelf.transform.position;
        shelfPos.x -= 5f;
        shelf.transform.position = shelfPos;
        clear_UnderGround = true;
        clue_P.SetActive(true);
        ChapterEndingManager.Instance.CollectCh2Clue("P");
        ConsumeClue(clueitems);
        UIManager.Instance.PromptUI.ShowPrompt_2("으악...!!", "벽에 뭔가 나타났어...!");
    }

    void ConsumeClue(List<ClueData> clues) {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
    }

    void CancleScan(string reason) {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
    }

    override protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && !clear_UnderGround) {
            isNearMemory = true;
            currentScanObject = collision.gameObject;
            player.InteractSystem.AddInteractable(gameObject);  
            // Vector3 e_keyPos = transform.position; 
            // e_keyPos.y += 0.3f;
            // e_key.transform.position = e_keyPos;
            // e_key.SetActive(true);

            // currentMemoryFragment = currentScanObject.GetComponent<MemoryFragment>();
        }
    }

    override protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            isNearMemory = false;
            player.InteractSystem.RemoveInteractable(gameObject);
            // 스캔 중에 범위를 벗어났다면 스캔을 취소
            if (isScanning) {
                CancleScan("범위를 이탈하여 스캔이 중단되었습니다.");
            }

            currentScanObject = null;
        }
    }
}