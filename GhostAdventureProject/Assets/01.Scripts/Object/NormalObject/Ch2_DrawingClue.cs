using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ch2_DrawingClue : MonoBehaviour
{
    [Header("프롬프트 메시지 설정")] [SerializeField]
    private string promptMessage;
    
    [SerializeField] private GameObject drawingZoom;             // 확대용 UI (Canvas 내)
    [SerializeField] private RectTransform drawingPos;           // 시작 위치
    [SerializeField] private Image zoomPanel;                    // 배경 패널 (페이드용)
    [SerializeField] private GameObject nextClueObject;

    private CluePickup cluePickup;
    private bool isPlayerInside = false;
    private bool isZoomActive = false;
    private bool zoomActivatedOnce = false;
    [SerializeField] private bool hasActivated = false;
    [SerializeField] private bool isLastClue = false;
    [SerializeField] private Ch2_BackStreetObj finalObjectToActivate; 

    void Start()
    {
        cluePickup = GetComponent<CluePickup>();

        // 초기화
        drawingZoom.SetActive(false);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!hasActivated || (!isZoomActive && !isPlayerInside))
                return;

            if (isZoomActive)
                HideDrawingZoom();
            else
                ShowDrawingZoom();
        }
    }

    private void ShowDrawingZoom()
    {
        isZoomActive = true;
        EnemyAI.PauseAllEnemies();

        // 배경 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // UI 슬라이드 인
        drawingZoom.SetActive(true);
        drawingPos.anchoredPosition = new Vector2(0, -Screen.height);
        drawingPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
        
        if (cluePickup != null)
        {
            cluePickup.PickupClue();
        }
        
        if (!string.IsNullOrEmpty(promptMessage))
        {
            UIManager.Instance.PromptUI.ShowPrompt(promptMessage, 2f);
        }
    }

    private void HideDrawingZoom()
    {
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

                if (!zoomActivatedOnce)
                {
                    // cluePickup?.PickupClue();
                    
                    if (isLastClue && finalObjectToActivate != null)
                    {
                        // finalObjectToActivate.SendMessage("OnFinalClueActivated");
                        finalObjectToActivate.OnFinalClueActivated();
                    }

                    if (nextClueObject != null)
                    {
                        var nextClue = nextClueObject.GetComponent<Ch2_DrawingClue>();
                        if (nextClue != null)
                            nextClue.Activate();
                    }
                    
                    zoomActivatedOnce = true;
                    GetComponent<Collider2D>().enabled = false;
                }

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated || !other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideDrawingZoom(); // 범위 밖이면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
    
    public void Activate()
    {
        hasActivated = true;
    }
}
