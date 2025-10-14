using UnityEngine;
using UnityEngine.UI;
using static _01.Scripts.Utilities.Timer;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_ScanToTeleport : MonoBehaviour
    {
        [Header("References")] 
        [SerializeField] GameObject scanPanel;  //스캔 패널
        [SerializeField] Image scanCircleUI;    //스캔 원 UI

        [Header("Target to Teleport")] 
        [SerializeField] Transform target;
        
        [Header("Scan Settings")]
        [SerializeField] float scanDuration = 2f;
        
        Camera cam;
        CountdownTimer scanTimer;
        Player.Player player;
        bool alreadyScanned;
        bool isScanning;
        bool isScanKeydown;
        bool isTeleportable;
        
        void Start() {
            cam = Camera.main;
            player = GameManager.Instance.Player;
            scanPanel = UIManager.Instance.scanUI;
            scanCircleUI = UIManager.Instance.scanUI.transform.Find("CircleUI")?.GetComponent<Image>();
            scanCircleUI?.gameObject.SetActive(false);
            
            // Initialize Countdown Timer
            scanTimer = new CountdownTimer(scanDuration) {
                OnTimerStart = StartScan,
                OnTimerStop = () => {
                    if (scanTimer.IsFinished) CompleteScan();
                    else CancelScan("스캔이 중단");
                }
            };
        }

        void Update() {
            isScanKeydown = Input.GetKey(KeyCode.E);

            if (isTeleportable && !alreadyScanned && !isScanning && isScanKeydown) TryStartScan();

            // Tick timer & Update Scan Progress bar
            scanTimer.Tick(Time.unscaledDeltaTime);
            if (isScanning) UpdateScan();
        }
        
        void LateUpdate() {
            if (scanCircleUI && scanCircleUI.gameObject.activeInHierarchy) {
                scanCircleUI.transform.position = cam.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
            }
        }
        
        public void SetTeleportable(bool teleportable) => isTeleportable = teleportable;
        
        public bool GetTeleportable() => isTeleportable;

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
            isScanning = false;

            Time.timeScale = 1f;

            scanCircleUI?.gameObject.SetActive(false);
            scanPanel?.SetActive(false);
            UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();

            TeleportPlayer();
        }

        void CancelScan(string reason) {
            Debug.Log(reason);
            isScanning = false;
            Time.timeScale = 1f; // 시간 흐름을 원래대로 복구

            scanPanel?.SetActive(false);
            scanCircleUI?.gameObject.SetActive(false);
            UIManager.Instance.PromptUI2.HidePrompt_UnPlayMode();
        }

        void TeleportPlayer() {
            if (!target) return;

            GameManager.Instance.Player.transform.position = target.position;
        }
        
        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                isTeleportable = true;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                isTeleportable = false;
            }
        }
    }
}