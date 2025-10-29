using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch4_DollReveal : MonoBehaviour
{
    [Header("Puzzle Binding")]
    [SerializeField] Ch4_SpiderPuzzleController controller;
    [SerializeField] Ch4_PatternAsset patternForThisDoll;

    [Header("Pattern Emblem")]
    [SerializeField] SpriteRenderer emblemRenderer;

    [Header("Rig References")]
    [Tooltip("천장에 고정된 점 (안 움직임)")]
    [SerializeField] Transform ceilingPoint;

    [Tooltip("실제 인형 몸통(= SPUM 본체 루트). 얘가 위아래로 움직임")]
    [SerializeField] Transform dollBody;

    [Tooltip("라인렌더러로 된 줄 비주얼 (optional). 없으면 null 가능")]
    [SerializeField] LineRenderer ropeLine;

    [Header("Tween Settings")]
    [SerializeField] bool useLocalPosition = false;

    [Tooltip("아래로 얼마나 내려올지 (양수면 아래)")]
    [SerializeField] float dropDistance = 2.0f;

    [Tooltip("내려오는 시간")]
    [SerializeField] float dropDuration = 0.6f;

    [Tooltip("올라가는 시간")]
    [SerializeField] float raiseDuration = 0.5f;

    [Tooltip("내려올 때 탄성 (통통 튀는 느낌 주려면 OutBounce 등)")]
    [SerializeField] Ease lowerEase = Ease.OutBounce;

    [Tooltip("올라갈 때 탄성 (OutBack 등)")]
    [SerializeField] Ease raiseEase = Ease.OutBack;

    [Tooltip("OutBack 탄성 정도")]
    [SerializeField] float raiseOvershoot = 1.2f;

    [Header("Optional Override")]
    [Tooltip("내려왔을 때 최종 위치(로컬 또는 월드 y)를 직접 지정하고 싶으면 이걸 씀")]
    [SerializeField] Transform customDropPoint;

    // 내부 상태
    Vector3 startPosW;
    Vector3 startPosL;
    Tween activeTween;

    void Awake()
    {
        DOTween.defaultEaseOvershootOrAmplitude = raiseOvershoot;

        // dollBody를 반드시 기준으로 삼는다.
        if (dollBody == null)
            dollBody = transform; // 안전장치: 기존처럼 자기 자신을 쓸 수도 있게

        startPosW = dollBody.position;
        startPosL = dollBody.localPosition;

        // 문양 아이콘 초기화 (패턴 사전 세팅해놨으면 여기서 한번 적용해도 됨)
        if (patternForThisDoll != null)
        {
            ApplyPatternSprite(patternForThisDoll);
        }

        // ropeLine 초기화
        UpdateRopeVisual();
    }

    void OnDisable()
    {
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();
        activeTween = null;
    }

    // ==========================
    // 패턴 아이콘 세팅
    // ==========================
    public void SetPattern(Ch4_PatternAsset p)
    {
        patternForThisDoll = p;
        ApplyPatternSprite(p);
    }

    void ApplyPatternSprite(Ch4_PatternAsset p)
    {
        if (!emblemRenderer || p == null) return;

        emblemRenderer.sprite = p.wallSymbolSprite ? p.wallSymbolSprite : p.noteSprite;

        var c = emblemRenderer.color;
        c.a = 1f;
        emblemRenderer.color = c;

        emblemRenderer.enabled = true;
        emblemRenderer.sortingOrder = 50;
    }

    // ==========================
    // 인형을 "줄에 매달려서" 내려오게
    // ==========================
    public void Lower()
    {
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        Vector3 targetPosWorld;
        Vector3 targetPosLocal;

        if (useLocalPosition)
        {
            float targetY = customDropPoint
                ? customDropPoint.localPosition.y
                : startPosL.y - Mathf.Abs(dropDistance);

            targetPosLocal = new Vector3(startPosL.x, targetY, startPosL.z);

            // 로컬 트윈
            activeTween = dollBody.DOLocalMove(targetPosLocal, dropDuration)
                .SetEase(lowerEase)
                .OnUpdate(UpdateRopeVisual)  // 내려오는 동안 줄 길이 계속 갱신
                .OnComplete(() => {
                    UpdateRopeVisual();
                });
        }
        else
        {
            float targetY = customDropPoint
                ? customDropPoint.position.y
                : startPosW.y - Mathf.Abs(dropDistance);

            targetPosWorld = new Vector3(startPosW.x, targetY, startPosW.z);

            // 월드 트윈
            activeTween = dollBody.DOMove(targetPosWorld, dropDuration)
                .SetEase(lowerEase)
                .OnUpdate(UpdateRopeVisual)
                .OnComplete(() => {
                    UpdateRopeVisual();
                });
        }
    }

    // ==========================
    // 다시 올라가게 (줄이 감기는 느낌)
    // ==========================
    public void Raise()
    {
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        if (useLocalPosition)
        {
            activeTween = dollBody.DOLocalMove(startPosL, raiseDuration)
                .SetEase(raiseEase)
                .OnUpdate(UpdateRopeVisual)
                .OnComplete(() => {
                    UpdateRopeVisual();
                });
        }
        else
        {
            activeTween = dollBody.DOMove(startPosW, raiseDuration)
                .SetEase(raiseEase)
                .OnUpdate(UpdateRopeVisual)
                .OnComplete(() => {
                    UpdateRopeVisual();
                });
        }
    }

    // ==========================
    // 최종 퍼즐 입력 (플레이어가 인형 건드리면 순서 보고)
    // ==========================
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!controller) return;

        if (other.CompareTag("Player"))
        {
            controller.RegisterDollPress(patternForThisDoll);

            // 인형이 살짝 흔들릴 때 줄도 같이 따라가게 하려면,
            // 흔들림은 dollBody 기준으로만 주고,
            // UpdateRopeVisual()를 LateUpdate에서 계속 호출해주면 된다.
            dollBody.DOShakePosition(0.25f, 0.1f, 10, 90f)
                .OnUpdate(UpdateRopeVisual)
                .OnComplete(UpdateRopeVisual);
        }
    }

    // ==========================
    // 줄 비주얼 갱신
    // ==========================
    void UpdateRopeVisual()
    {
        if (!ropeLine) return;
        if (!ceilingPoint) return;
        if (!dollBody) return;

        // 줄은 단순히 시작점=천장, 끝점=인형 몸 위치 로 그려주면 된다.
        ropeLine.positionCount = 2;
        ropeLine.SetPosition(0, ceilingPoint.position);
        ropeLine.SetPosition(1, dollBody.position);
    }

    // 만약 LateUpdate로 계속 줄을 따라가게 하고 싶다면:
    void LateUpdate()
    {
        UpdateRopeVisual();
    }
}
