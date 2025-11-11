using System.Collections.Generic;
using UnityEngine;
using TMPro;
using DG.Tweening;
using System.Threading.Tasks;
using System;

// Player의 대사를 출력하는 스크립트입니다.

public class Prompt : MonoBehaviour
{
    [Header("Player용 프롬프트")]
    [SerializeField] private RectTransform PromptContainer;   // 프롬프트가 들어갈 부모
    [SerializeField] private GameObject PromptPrefab;         // TMP_Text + CanvasGroup 포함

    [Header("Player용 - Anim Settings")]
    [SerializeField] private float spawnScale = 0.95f;        // 처음 크게
    [SerializeField] private float settleScale = 1.0f;        // 자리잡는 기본 크기
    [SerializeField] private float shiftUpY = 48f;            // 기존 프롬프트가 위로 밀리는 거리
    [SerializeField] private float shrinkFactor = 0.85f;      // 기존 프롬프트 스케일 축소 비율
    [SerializeField] private float fadeFactor = 0.65f;        // 기존 프롬프트 알파 감소 비율
    [SerializeField] private Color oldColor = new Color(1f,1f,1f,0.75f); // 이전 프롬프트 목표 색(옅은 회색 느낌)
    [SerializeField] private int maxVisible = 2;              // 화면에 남겨둘 최대 줄 수
    [SerializeField] private float spawnInTime = 0.25f;       // 새 프롬프트 등장 트윈
    [SerializeField] private float shiftTime = 0.25f;         // 기존 프롬프트 이동/축소/페이드
    [SerializeField] private Ease easeIn = Ease.OutCubic;
    [SerializeField] private Ease easeShift = Ease.OutCubic;

    private readonly List<PromptItem> items = new List<PromptItem>();

    private class PromptItem
    {
        public RectTransform rt;
        public TMP_Text text;
        public CanvasGroup cg;
        public float baseY;     // 현재 줄의 기준 Y (누적 이동 계산용)
        public float scale;     // 현재 스케일
        public float alpha;     // 현재 알파
        public Tween running;   // 현재 적용중인 트윈(있으면 Kill)
        public Tween shift;     // 위로 이동/축소/옅어짐용 (ShowPrompt가 호출될 때마다 갱신)
        public Sequence life;
    }

    // 외부 API 그대로 사용
    public void ShowPrompt(string line, float delaytime = 2f)
    {
        // 1) 기존 프롬프트들 “위로 이동 + 작게 + 옅게”
        for (int i = items.Count - 1; i >= 0; i--)
        {
            var it = items[i];

            it.baseY += shiftUpY;
            it.scale *= shrinkFactor;
            it.alpha *= fadeFactor;

            Color targetColor = oldColor;
            targetColor.a = it.alpha;

            it.shift?.Kill(); // 이동/축소/색상만 덮어씀
            it.shift = DOTween.Sequence()
                .Join(it.rt.DOAnchorPosY(it.baseY, shiftTime).SetEase(easeShift))
                .Join(it.rt.DOScale(it.scale, shiftTime).SetEase(easeShift))
                .Join(it.cg.DOFade(it.alpha, shiftTime))
                .Join(it.text.DOColor(targetColor, shiftTime))
                .SetLink(it.rt.gameObject);
        }

        // 2) 새 프롬프트 인스턴스 생성 (맨 아래, 크게, 투명)
        var go = Instantiate(PromptPrefab, PromptContainer);
        var rt = go.GetComponent<RectTransform>();
        var txt = go.GetComponentInChildren<TMP_Text>();
        var cg  = go.GetComponent<CanvasGroup>();
        if (cg == null) cg = go.AddComponent<CanvasGroup>();

        txt.text = line;
        cg.alpha = 0f;

        // 새 항목 초기 상태
        rt.anchoredPosition = new Vector2(0f, 0f); // 컨테이너 하단 기준
        rt.localScale = Vector3.one * spawnScale;

        var newItem = new PromptItem { rt = rt, text = txt, cg = cg, baseY = 0f, scale = settleScale, alpha = 1f };
        items.Add(newItem);

        // 3) 새 프롬프트 “등장 애니메이션”(크게 들어와서 settleScale로)
        newItem.shift?.Kill();
        newItem.shift = DOTween.Sequence()
            .Join(cg.DOFade(1f, spawnInTime).SetEase(easeIn))
            .Join(rt.DOScale(settleScale, spawnInTime).SetEase(easeIn))
            .SetLink(go);
                // 4) 수명 관리: delaytime 뒤에 자동 삭제(최신 라인은 사라지고 위 라인들은 더 오래 남음)
                newItem.life?.Kill();
                newItem.life = DOTween.Sequence()
                    .AppendInterval(delaytime)               // 여기서 '대기'
                    .Append(cg.DOFade(0f, 1f))            // 사라지기
                    // .Join(rt.DOAnchorPosY(-12f, 0.2f))      // 살짝 내려가며
                    .OnComplete(() => {
                        items.Remove(newItem);
                        if (newItem.rt) Destroy(newItem.rt.gameObject);
                    })
            .SetLink(go);
        // .SetUpdate(true); // 패널 비활성/일시정지 중에도 카운트 하려면 활성화        });

        // 5) 너무 많아지면 오래된 것부터 정리
        CompactIfNeeded();
    }

    private void CompactIfNeeded()
    {
        while (items.Count > maxVisible)
        {
            var oldest = items[0];
            oldest.running?.Kill();
            if (oldest.rt != null) Destroy(oldest.rt.gameObject);
            items.RemoveAt(0);
        }
    }

    public async void ShowPrompt_2 (params string[] lines)
    {
        for (int i=0; i<lines.Length; i++)
        {
            ShowPrompt(lines[i]);
            await Task.Delay(2000);
        }
    }

     public void ShowPrompt_Random(params string[] lines)
    {
        string chosenLine = lines[UnityEngine.Random.Range(0, lines.Length)];
        ShowPrompt(chosenLine);
    }
}


