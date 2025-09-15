using _01.Scripts.Player;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch2_DrawingClue : MonoBehaviour
{
    [Header("프롬프트 메시지 설정")] 
    [SerializeField] string promptMessage;
    [SerializeField] GameObject drawingZoom; // 확대용 UI (Canvas 내)
    [SerializeField] RectTransform drawingPos; // 시작 위치
    [SerializeField] Image zoomPanel; // 배경 패널 (페이드용)
    [SerializeField] GameObject nextClueObject;

    [Header("Object State")]
    [SerializeField] bool hasActivated;
    [SerializeField] bool isLastClue;
    [SerializeField] Ch2_BackStreetObj finalObjectToActivate;
    
    [Header("Camera")]
    [SerializeField] CinemachineVirtualCamera vcam;

    CluePickup cluePickup;
    Player player;
    bool isPlayerInside;
    bool isZoomActive;
    bool zoomActivatedOnce;
    
    public bool HasActivated => hasActivated; // 저장용 getter
    public void ApplyHasActivatedFromSave(bool v) => hasActivated = v; // 복원용 setter

    void Start() {
        cluePickup = GetComponent<CluePickup>();
        player = GameManager.Instance.Player;
        
        // 초기화
        drawingZoom.SetActive(false);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!hasActivated || !isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
                HideDrawingZoom();
            else
                ShowDrawingZoom();
        }
    }

    void ShowDrawingZoom() {
        isZoomActive = true;
        EnemyAI.PauseAllEnemies();

        // 배경 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // UI 슬라이드 인
        drawingZoom.SetActive(true);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
        drawingPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        player.InteractSystem.RemoveInteractable(gameObject);

        if (cluePickup != null) {
            cluePickup.PickupClue();
        }

        if (!string.IsNullOrEmpty(promptMessage)) {
            UIManager.Instance.PromptUI.ShowPrompt(promptMessage);
        }
    }

    void HideDrawingZoom() {
        isZoomActive = false;
        EnemyAI.ResumeAllEnemies();

        // 페이드 아웃
        zoomPanel.DOFade(0f, 0.5f);

        // UI 슬라이드 아웃
        drawingPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                drawingZoom.SetActive(false);

                if (!zoomActivatedOnce) {
                    // cluePickup?.PickupClue();

                    if (isLastClue && finalObjectToActivate != null) {
                        PlayFinalLightSequence();
                    }

                    if (nextClueObject != null) {
                        Ch2_DrawingClue nextClue = nextClueObject.GetComponent<Ch2_DrawingClue>();
                        if (nextClue != null)
                            nextClue.Activate();
                    }

                    zoomActivatedOnce = true;
                    GetComponent<Collider2D>().enabled = false;
                }

                if (isPlayerInside)
                    player.InteractSystem.AddInteractable(gameObject);
            });
    }

    void PlayFinalLightSequence() {
        // 1) 플레이어 잠금 + VCam Follow 끊기
        player.PossessionSystem.CanMove = false;
        EnemyAI.PauseAllEnemies();
        Transform oldFollow = vcam.Follow;
        vcam.Follow = null;

        // 2) VCam 트랜스폼 & 위치 계산
        Transform camTrans = vcam.transform;
        Vector3 orig = camTrans.position;
        Vector3 dest = finalObjectToActivate.transform.position;
        dest.z = orig.z;

        // 3) 시퀀스 구성
        float moveT = 0.8f;
        float revealT = finalObjectToActivate.fadeInTime
                        + finalObjectToActivate.holdTime
                        + finalObjectToActivate.fadeOutTime
                        + finalObjectToActivate.moveDownTime;

        DOTween.Sequence()
            // 카메라(VCam) 이동
            .Append(camTrans.DOMove(dest, moveT).SetEase(Ease.InOutSine))
            // 단서 애니메이션 시작
            .AppendCallback(() => finalObjectToActivate.OnFinalClueActivated())
            // 애니메이션 전체 시간 대기
            .AppendInterval(revealT)
            // 카메라(VCam) 원위치 복귀
            .Append(camTrans.DOMove(orig, moveT).SetEase(Ease.InOutSine))
            // Follow 복구 & 플레이어 언락
            .AppendCallback(() =>
            {
                vcam.Follow = oldFollow;
                player.PossessionSystem.CanMove = true;
                EnemyAI.ResumeAllEnemies();
            });
    }

    void OnTriggerEnter2D(Collider2D other) {
        if (!hasActivated || !other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            player.InteractSystem.AddInteractable(gameObject);
    }

    void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideDrawingZoom(); // 범위 밖이면 자동 닫기

        player.InteractSystem.RemoveInteractable(gameObject);
    }

    public void Activate() {
        hasActivated = true;
    }
}