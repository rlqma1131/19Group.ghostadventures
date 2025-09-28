using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using static _01.Scripts.Utilities.Timer;

/// <summary>
///     Chapter 2의 인형 에셋에 들어갈 클래스
/// </summary>
public class Ch2_Doll : BaseInteractable
{
    [Header("References")] 
    [SerializeField] GameObject scanPanel;  //스캔 패널
    [SerializeField] Image scanCircleUI;    //스캔 원 UI
    [SerializeField] Ch2_Doll correctDoll;  // 정답 인형

    [Header("References for Event")] 
    [SerializeField] GameObject door; // 지하수로와 연결된 문
    [SerializeField] GameObject shelf; // 문 막고 있는 책장
    [SerializeField] GameObject clue_P; // 단서 P
    [SerializeField] List<ClueData> clueitems; // 소모되는 단서 아이템
    [SerializeField] SoundEventConfig soundConfig;

    [Header("Scan Settings")] 
    [SerializeField] bool isFake;
    [SerializeField] float scan_duration = 2f; //스캔 시간
    [SerializeField] bool isScannable;

    CountdownTimer scanTimer;
    Camera cam;
    bool isScanning;
    bool isNearMemory;
    bool isScanKeydown;

    public bool isOpen_UnderGroundDoor { get; private set; }
    public bool IsScannable => isScannable;

    override protected void Start() {
        base.Start();

        cam = Camera.main;
        scanPanel = UIManager.Instance.scanUI;
        scanCircleUI = UIManager.Instance.scanUI.transform.Find("CircleUI")?.GetComponent<Image>();
        scanCircleUI?.gameObject.SetActive(false);
        if (clue_P) clue_P.SetActive(false);

        // Initialize Countdown Timer
        scanTimer = new CountdownTimer(scan_duration);
        scanTimer.OnTimerStart = StartScan;
        scanTimer.OnTimerStop = () =>
        {
            if (scanTimer.IsFinished) CompleteScan();
            else CancelScan("스캔이 중단");
        };

        // If reference of answer is missing, set doll as unscannable.
        if (!correctDoll && isFake) {
            Debug.LogError("Reference of Correct Doll is Missing!");
            return;
        }

        isScannable = true;
    }

    void Update() {
        // 지하수로 문이 열렸다면 스캔불가능하게 변경
        if (isFake) {
            if (correctDoll.isOpen_UnderGroundDoor && isScannable) isScannable = false;
        }
        else if (isOpen_UnderGroundDoor && isScannable) {
            isScannable = false;
        }

        isScanKeydown = Input.GetKey(KeyCode.E);

        if (isNearMemory && isScannable && !isScanning && isScanKeydown) TryStartScan();

        // Tick timer & Update Scan Progress bar
        scanTimer.Tick(Time.unscaledDeltaTime);
        if (isScanning) UpdateScan();
    }

    void LateUpdate() {
        if (scanCircleUI && scanCircleUI.gameObject.activeInHierarchy) {
            scanCircleUI.transform.position = cam.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
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
        isScanning = true;
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
        if (isScanKeydown) scanCircleUI.fillAmount = 1f - scanTimer.Progress;
        else scanTimer.Stop();
    }

    void CompleteScan() {
        isNearMemory = false;
        isScannable = false;
        isScanning = false;

        Time.timeScale = 1f;

        scanCircleUI?.gameObject.SetActive(false);
        scanPanel?.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();

        if (!isFake) {
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
        else {
            if (soundConfig)
                SoundTrigger.TriggerSound(transform.position, soundConfig.soundRange, soundConfig.chaseDuration);
        }
    }

    void CancelScan(string reason) {
        Debug.Log(reason);
        isScanning = false;
        Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

        scanPanel?.SetActive(false);
        scanCircleUI?.gameObject.SetActive(false);
        UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();
    }

    void ConsumeClue(List<ClueData> clues) => UIManager.Instance.Inventory_PlayerUI.RemoveClue(clues.ToArray());

    override protected void OnTriggerEnter2D(Collider2D collision) {
        if (collision.CompareTag("Player") && isScannable) {
            isNearMemory = true;
            player.InteractSystem.AddInteractable(gameObject);
        }
    }

    override protected void OnTriggerExit2D(Collider2D collision) {
        if (collision.CompareTag("Player")) {
            isNearMemory = false;
            player.InteractSystem.RemoveInteractable(gameObject);
            scanTimer.Stop();
        }
    }
}