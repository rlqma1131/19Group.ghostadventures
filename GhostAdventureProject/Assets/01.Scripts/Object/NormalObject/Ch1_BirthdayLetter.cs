using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class Ch1_BirthdayLetter : MonoBehaviour
{
    [SerializeField] private GameObject letterZoom;
    [SerializeField] private RectTransform letterPosition; 
    [SerializeField] private Image zoomPanel;           

    private bool isZoomActive = false;
    private bool zoomActivatedOnce = false;
    private bool isPlayerInside = false;

    private void Start()
    {
        // UI 컴포넌트 초기화
        letterZoom = GameObject.Find("Ch1_BirthdayLetterZoom");
        letterPosition = letterZoom.GetComponent<RectTransform>();
        zoomPanel = GameObject.Find("ZoomPanel").GetComponent<Image>();

        // 초기화
        letterZoom.SetActive(false);
        letterPosition.anchoredPosition = new Vector2(0, -Screen.height);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
                HideLetterZoom();
            else
                ShowLetterZoom();
        }
    }

    private void ShowLetterZoom()
    {
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        letterZoom.SetActive(true);
        letterPosition.anchoredPosition = new Vector2(0, -Screen.height);
        letterPosition.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideLetterZoom()
    {
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        letterPosition.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                letterZoom.SetActive(false);

                if (!zoomActivatedOnce)
                {
                    Ch1_HideAreaEvent.Instance.RestoreHideAreaTags();
                    zoomActivatedOnce = true;
                }

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideLetterZoom();

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
