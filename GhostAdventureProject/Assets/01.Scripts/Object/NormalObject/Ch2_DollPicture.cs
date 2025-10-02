using System.Collections;
using System.Collections.Generic;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch2_DollPicture : BaseInteractable
{
    [SerializeField] GameObject ZoomImage; // 확대용 UI (Canvas 내)
    [SerializeField] RectTransform drawingPos; // 시작 위치
    [SerializeField] Image zoomPanel; // 배경 패널 (페이드용)

    [Header("Object State")]
    [SerializeField] Ch2_BackStreetObj finalObjectToActivate;
    
    [Header("Camera")]
    [SerializeField] CinemachineVirtualCamera vcam;

    CluePickup cluePickup;
    bool isPlayerInside;
    bool isZoomActive;
    bool zoomActivatedOnce;
    [SerializeField] bool isFake;

    override protected void Start() {
        base.Start();
        cluePickup = GetComponent<CluePickup>();
        
        // 초기화
        ZoomImage.SetActive(false);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update() {
        if (Input.GetKeyDown(KeyCode.E)) {
            if (!isZoomActive && !isPlayerInside) return;

            TriggerEvent();
        }
    }

    public override void TriggerEvent() {
        if (isZoomActive) HideDrawingZoom();
        else ShowDrawingZoom();
    }

    void ShowDrawingZoom() {
        isZoomActive = true;
        EnemyAI.PauseAllEnemies();

        // 배경 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // UI 슬라이드 인
        ZoomImage.SetActive(true);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
        drawingPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        player.InteractSystem.RemoveInteractable(gameObject);

        if (cluePickup != null) {
            cluePickup.PickupClue();
        }

        if(isFake) UIManager.Instance.PromptUI.ShowPrompt("모래성에서 본 그림과 다른데… 하나는 가짜야.");
        else UIManager.Instance.PromptUI.ShowPrompt("여자아이 그림...?");
    }
    void HideDrawingZoom() {
        isZoomActive = false;
        EnemyAI.ResumeAllEnemies();

        // 페이드 아웃
        zoomPanel.DOFade(0f, 0.5f);

        // UI 슬라이드 아웃
        drawingPos
            .DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() => {
                ZoomImage.SetActive(false);

                if (!zoomActivatedOnce) {
                    // cluePickup?.PickupClue();

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

    override protected void OnTriggerEnter2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;
        if (!isZoomActive) player.InteractSystem.AddInteractable(gameObject);
    }
    override protected void OnTriggerExit2D(Collider2D other) {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;
        if (isZoomActive) HideDrawingZoom(); // 범위 밖이면 자동 닫기
        player.InteractSystem.RemoveInteractable(gameObject);
    }

}
