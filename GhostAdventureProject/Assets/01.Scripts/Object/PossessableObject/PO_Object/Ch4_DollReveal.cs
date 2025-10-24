using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class Ch4_DollReveal : MonoBehaviour
{
    [Header("Puzzle Binding")]
    [SerializeField] Ch4_SpiderPuzzleController controller;
    [SerializeField] Ch4_PatternAsset patternForThisDoll;

    [SerializeField] SpriteRenderer emblemRenderer; // 인형에 표시될 문양(스프라이트)

    [Header("Tween Settings")]
    [Tooltip("시작(원래) 위치를 로컬좌표로 해석할지 여부")]
    [SerializeField] bool useLocalPosition = false;

    [Tooltip("아래로 떨어질 거리(+값이면 아래로 이동)")]
    [SerializeField] float dropDistance = 2.0f;

    [Tooltip("떨어질 때 소요 시간")]
    [SerializeField] float dropDuration = 0.6f;

    [Tooltip("올라갈 때 소요 시간")]
    [SerializeField] float raiseDuration = 0.5f;

    [Tooltip("떨어질 때 바운스 강도(Ease.OutBounce 추천)")]
    [SerializeField] Ease lowerEase = Ease.OutBounce;

    [Tooltip("올라갈 때 탄성(Ease.OutBack 추천)")]
    [SerializeField] Ease raiseEase = Ease.OutBack;

    [Tooltip("OutBack 탄성 정도(0~3 정도)")]
    [SerializeField] float raiseOvershoot = 1.2f;

    [Header("Optional")]
    [Tooltip("떨어질 목표 위치를 직접 지정하고 싶으면 여기에 할당(거리 대신 이 위치의 Y로 이동)")]
    [SerializeField] Transform customDropPoint;

    // 내부 상태
    Vector3 startPosW;   // 월드 기준 시작 위치
    Vector3 startPosL;   // 로컬 기준 시작 위치
    Tween activeTween;

    void Awake()
    {
        // 시작 위치 저장
        startPosW = transform.position;
        startPosL = transform.localPosition;

        // OutBack 계열의 overshoot 설정
        DOTween.defaultEaseOvershootOrAmplitude = raiseOvershoot;
    }

    void OnDisable()
    {
        // 안전: 게임오브젝트 비활성 시 트윈 정리
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();
        activeTween = null;
    }

    public Ch4_PatternAsset Pattern => patternForThisDoll;

    public void SetPattern(Ch4_PatternAsset p)
    {
        patternForThisDoll = p;
        if (emblemRenderer && p)
        {
            // wallSymbolSprite가 없으면 noteSprite로 대체
            emblemRenderer.sprite = p.wallSymbolSprite ? p.wallSymbolSprite : p.noteSprite;

            // 혹시 투명/비활성 상태였다면 보이게
            var c = emblemRenderer.color;
            c.a = 1f;
            emblemRenderer.color = c;
            emblemRenderer.enabled = true;
            emblemRenderer.sortingOrder = 50; // 다른 것 위로 오게 높여두면 안전
        }
    }

    public void Lower()
    {
        // 현재 트윈 정리
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        if (useLocalPosition)
        {
            float targetY = customDropPoint
                ? customDropPoint.localPosition.y
                : startPosL.y - Mathf.Abs(dropDistance);

            var target = new Vector3(startPosL.x, targetY, startPosL.z);
            activeTween = transform.DOLocalMove(target, dropDuration).SetEase(lowerEase);
        }
        else
        {
            float targetY = customDropPoint
                ? customDropPoint.position.y
                : startPosW.y - Mathf.Abs(dropDistance);

            var target = new Vector3(startPosW.x, targetY, startPosW.z);
            activeTween = transform.DOMove(target, dropDuration).SetEase(lowerEase);
        }
    }

    public void Raise()
    {
        if (activeTween != null && activeTween.IsActive()) activeTween.Kill();

        if (useLocalPosition)
        {
            activeTween = transform.DOLocalMove(startPosL, raiseDuration).SetEase(raiseEase);
        }
        else
        {
            activeTween = transform.DOMove(startPosW, raiseDuration).SetEase(raiseEase);
        }
    }

    // -------------------------
    // 🔽 최종 퍼즐 입력 처리 부분 추가
    // 바람(또는 플레이어)이 인형에 닿으면 이 인형의 패턴을 컨트롤러에 보고한다.
    // -------------------------
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!controller) return;

        // 여기서 어떤 태그로 눌리게 할지 결정.
        // 유령의 바람이 "Wind" 태그면 Wind 추가, 직접 본체로 밀면 Player 등 원하는 태그 써.
        if (other.CompareTag("Player"))
        {
            controller.RegisterDollPress(patternForThisDoll);

            // 피드백(작게 흔들리게): 선택사항
            transform.DOShakePosition(0.25f, 0.1f, 10, 90f);
        }
    }
}
