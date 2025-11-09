using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ChapterChoiceBtn : MonoBehaviour
{
    [Header("🎬 버튼 & 패널 설정")]
    [SerializeField] private Button triggerButton;          // 챕터 선택 버튼
    [SerializeField] private Button closeButton;            // X 버튼
    [SerializeField] private CanvasGroup panelCanvasGroup;  // 패널 CanvasGroup
    [SerializeField] private RectTransform buttonPanel;     // 패널 RectTransform
    [SerializeField] private RectTransform[] targetButtons; // 띵띵띵띵 등장할 버튼들

    private Sequence seq;
    private bool isOpen = false;

    void Start()
    {
        // 처음엔 패널 비활성화
        panelCanvasGroup.alpha = 0;
        buttonPanel.localScale = Vector3.zero;
        buttonPanel.gameObject.SetActive(false);

        foreach (var btn in targetButtons)
            btn.localScale = Vector3.zero;

        // 이벤트 등록
        triggerButton.onClick.AddListener(TogglePanel);
        closeButton.onClick.AddListener(() =>
        {
            if (isOpen)
                TogglePanel();
        });
    }

    void TogglePanel()
    {
        if (seq != null && seq.IsActive()) seq.Kill();

        seq = DOTween.Sequence();

        if (!isOpen)
        {
            // 패널 열기
            buttonPanel.gameObject.SetActive(true);

            seq.Append(panelCanvasGroup.DOFade(1f, 0.4f));
            seq.Join(buttonPanel.DOScale(1f, 0.4f).SetEase(Ease.OutBack));

            // 패널이 열리고 나서 버튼 등장 시작
            seq.AppendCallback(() =>
            {
                foreach (var btn in targetButtons)
                {
                    CanvasGroup cg = btn.GetComponent<CanvasGroup>();
                    if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();
                    cg.alpha = 0;
                    btn.localScale = Vector3.zero;

                    seq.Append(
                        DOTween.Sequence()
                            .Append(cg.DOFade(4.8f, 0.25f))
                            .Join(btn.DOScale(4.8f, 0.25f).SetEase(Ease.OutBack))
                    );
                    seq.AppendInterval(0.05f); // 버튼 간 텀
                }
            });

            isOpen = true;
        }
        else
        {
            // 버튼 닫기 (역순)
            for (int i = targetButtons.Length - 1; i >= 0; i--)
            {
                var btn = targetButtons[i];
                CanvasGroup cg = btn.GetComponent<CanvasGroup>();
                if (cg == null) cg = btn.gameObject.AddComponent<CanvasGroup>();

                seq.Append(
                    DOTween.Sequence()
                        .Append(cg.DOFade(0f, 0.2f))
                        .Join(btn.DOScale(0f, 0.2f).SetEase(Ease.InBack))
                );
                seq.AppendInterval(0.05f);
            }

            // 버튼 사라지고 나서 패널 닫기
            seq.Append(panelCanvasGroup.DOFade(0f, 0.3f));
            seq.Join(buttonPanel.DOScale(0f, 0.3f).SetEase(Ease.InBack));

            seq.OnComplete(() =>
            {
                buttonPanel.gameObject.SetActive(false);
                isOpen = false;
            });
        }

        seq.Play();
    }
}
