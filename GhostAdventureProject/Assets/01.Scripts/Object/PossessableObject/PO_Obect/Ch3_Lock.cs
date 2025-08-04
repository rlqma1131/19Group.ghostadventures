using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

public class Ch3_Lock : BasePossessable
{
    [Header("확대 UI")]
    [SerializeField] private GameObject lockZoom; // 확대용 드로잉 UI (Canvas 하위)
    [SerializeField] private RectTransform lockPos; // 드로잉 UI의 시작 위치
    [SerializeField] private Image zoomPanel;        // 배경 패널 (알파 페이드용)

    [Header("확대 버튼UI들")]
    [SerializeField] private GameObject topBtn;
    [SerializeField] private GameObject bottomBtn;
    [SerializeField] private GameObject num1Btn;
    [SerializeField] private GameObject num2Btn;
    [SerializeField] private GameObject num3Btn;

    private Image top;
    private Image bottom;
    private Image num1;
    private Image num2;
    private Image num3;

    private bool isPlayerInside = false;
    private bool isZoomActive = false;

    protected override void Start()
    {
        base.Start();

        top = topBtn.GetComponent<Image>();
        bottom = bottomBtn.GetComponent<Image>();
        num1 = num1Btn.GetComponent<Image>();
        num2 = num2Btn.GetComponent<Image>();
        num3 = num3Btn.GetComponent<Image>();

        // UI 초기화
        lockZoom.SetActive(false);
        lockPos.anchoredPosition = new Vector2(0, -Screen.height);
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            if (!isZoomActive && !isPlayerInside)
                return;

            if (isZoomActive)
            {
                HideLockZoom();
            }
            else
            {
                ShowLockZoom();
            }
        }
    }

    private void ShowLockZoom()
    {
        isZoomActive = true;

        // 패널 페이드 인
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        // 슬라이드 인
        lockZoom.SetActive(true);
        lockPos.anchoredPosition = new Vector2(0, -Screen.height);
        lockPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }

    private void HideLockZoom()
    {
        isZoomActive = false;

        zoomPanel.DOFade(0f, 0.5f);

        lockPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                lockZoom.SetActive(false);

                if (isPlayerInside)
                    PlayerInteractSystem.Instance.AddInteractable(gameObject);
            });
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = true;

        if (!isZoomActive)
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
    }

    protected override void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag("Player")) return;

        isPlayerInside = false;

        if (isZoomActive)
            HideLockZoom(); // 플레이어가 나가면 자동 닫기

        PlayerInteractSystem.Instance.RemoveInteractable(gameObject);
    }
}
