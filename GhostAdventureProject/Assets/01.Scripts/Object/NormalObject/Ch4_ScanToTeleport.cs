using System.Collections.Generic;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using static _01.Scripts.Utilities.Timer;

namespace _01.Scripts.Object.NormalObject
{
    public class Ch4_ScanToTeleport : MonoBehaviour
    {
        #region Fields

        [Header("References")] 
        [SerializeField] GameObject scanPanel; //스캔 패널
        [SerializeField] Image scanCircleUI; //스캔 원 UI

        [Header("Teleport Settings")] 
        [SerializeField] Transform target;
        [SerializeField] List<string> linesWhenEntered = new();

        [Header("Scan Settings")] 
        [SerializeField] float scanDuration = 2f;
        
        [Header("Player Control Settings")]
        [SerializeField] bool isSlowdownAvailable;

        [Header("FadeOut/In Settings")] 
        [SerializeField] float fadeDuration = 4f;
        [SerializeField] UnityEvent onEnter;
        [SerializeField] UnityEvent onProgress;
        [SerializeField] UnityEvent onExit;

        // Component Fields
        Camera cam;
        CountdownTimer scanTimer;
        Player.Player player;
        
        // Value Fields
        bool alreadyScanned;
        bool isScanning;
        bool isScanKeydown;
        bool isTeleportable;
        bool isPlayerInside;
        bool isInTransition;

        #endregion
        
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

            if (isPlayerInside && !isInTransition && isTeleportable && 
                !alreadyScanned && !isScanning && isScanKeydown) TryStartScan();

            // Tick timer & Update Scan Progress bar
            scanTimer.Tick(Time.unscaledDeltaTime);
            if (isScanning) UpdateScan();
        }
        
        void LateUpdate() {
            if (scanCircleUI && scanCircleUI.gameObject.activeInHierarchy) {
                scanCircleUI.transform.position = cam.WorldToScreenPoint(player.transform.position) + new Vector3(-40, 50, 0);
            }
        }
        
        public void SetTeleportable(bool teleportable) {
            isTeleportable = teleportable;

            if (!target) return;
            if (!target.TryGetComponent(out Ch4_ScanToTeleport teleport)) return;
            if (teleport.GetTeleportable() == isTeleportable) return;
            
            teleport.SetTeleportable(isTeleportable);
        }

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
            
            UIManager.Instance.FadeOutIn(fadeDuration, 
                () => {
                    onEnter?.Invoke();
                    isInTransition = true;
                    
                    GameManager.Instance.Player.PossessionSystem.CanMove = false;
                }, 
                () => {
                    GameManager.Instance.Player.transform.position = target.position;
                },
                () => {
                    onExit?.Invoke();
                    isInTransition = false;
                    
                    if (linesWhenEntered.Count > 0) UIManager.Instance.PromptUI.ShowPrompt_2(linesWhenEntered.ToArray());
                    if (isSlowdownAvailable) {
                        bool val = GameManager.Instance.PlayerController.GetSlowdownAvailable();
                        GameManager.Instance.PlayerController.SetSlowdownAvailable(!val);
                    }
                    GameManager.Instance.Player.PossessionSystem.CanMove = true;
                });
        }
        
        void OnTriggerEnter2D(Collider2D collision) {
            if (collision.CompareTag("Player")) {
                isPlayerInside = true;
            }
        }

        void OnTriggerExit2D(Collider2D other) {
            if (other.CompareTag("Player")) {
                isPlayerInside = false;
            }
        }
    }
}