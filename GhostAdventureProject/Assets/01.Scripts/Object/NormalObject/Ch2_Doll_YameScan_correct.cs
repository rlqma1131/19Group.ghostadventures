using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Deprecated
/// </summary>
public class Ch2_Doll_YameScan_correct : BaseInteractable
{
    [SerializeField] float scan_duration = 2f;  // 스캔 시간
    [SerializeField] GameObject scanPanel;      // 스캔 패널
    [SerializeField] Image scanCircleUI;        // 스캔 원 UI
    [SerializeField] GameObject door;           // 지하수로와 연결된 문
    [SerializeField] GameObject shelf;          // 문 막고 있는 책장
    [SerializeField] GameObject clue_P;         // 단서 P
    [SerializeField] List<ClueData> clueitems;  // 소모되는 단서 아이템
    [SerializeField] bool isScannable;
    
    public bool isOpen_UnderGroundDoor;         // 지하수로 문이 열렸는지 확인
    float scanTime;
    bool isScanning;
    bool isNearMemory;
    
    protected override void Start() {
        base.Start();
        isScannable = true;

        scanPanel = UIManager.Instance.scanUI;
        scanCircleUI = UIManager.Instance.scanUI.transform.Find("CircleUI")?.GetComponent<Image>();
        scanCircleUI?.gameObject.SetActive(false);
        clue_P.SetActive(false);
    }

    void Update() {
        if (isNearMemory && isScannable && !isScanning && Input.GetKeyDown(KeyCode.E)) {
            TryStartScan();
        }

        if (isScanning) {
            UpdateScan();
        }
        
        if (door.activeInHierarchy && !isOpen_UnderGroundDoor) {
            isOpen_UnderGroundDoor = true;
        }
    }

    void LateUpdate() {
        if (scanCircleUI != null && scanCircleUI.gameObject.activeInHierarchy) {
            scanCircleUI.transform.position =
                Camera.main.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
        }
    }

    void TryStartScan() {
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

    void UpdateScan() {
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

    void CompleteScan() {
        isNearMemory = false;
        isScanning = false;
        isScannable = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();

        // 문 표시, 문을 막고있던 책장 옆으로 옮김
        door.SetActive(true);
        Vector3 shelfPos = shelf.transform.position;
        shelfPos.x -= 5f;
        shelf.transform.position = shelfPos;

        isOpen_UnderGroundDoor = true;
        clue_P.SetActive(true);
        ChapterEndingManager.Instance.CollectCh2Clue("P");
        ConsumeClue(clueitems);
        UIManager.Instance.PromptUI.ShowPrompt_2("으악...!!", "벽에 뭔가 나타났어...!");
    }

    void CancleScan(string reason) {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();
    }
    
    void ConsumeClue(List<ClueData> clues) {
        UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());
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