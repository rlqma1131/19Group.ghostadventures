using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverlayHoleFitter : MonoBehaviour
{
    [Header("Target")]
    public RectTransform allow;          // 구멍 내줄 버튼 RectTransform(다른 Canvas여도 OK)
    public float padding = 8f;           // 버튼 주변 여백

    [Header("Panels")]
    public RectTransform topPanel;
    public RectTransform bottomPanel;
    public RectTransform leftPanel;
    public RectTransform rightPanel;

    Canvas overlayCanvas;
    RectTransform overlayRect;

    void Awake()
    {
        overlayCanvas = GetComponentInParent<Canvas>();
        overlayRect   = GetComponent<RectTransform>();
        // 네 패널은 모두 Raycast Target ON 상태의 Image를 달아주세요.
        SetRectMode(topPanel);
        SetRectMode(bottomPanel);
        SetRectMode(leftPanel);
        SetRectMode(rightPanel);
    }

    void LateUpdate()
    {
        if (!allow || !overlayCanvas || !overlayRect) return;
        FitPanelsToMakeHole(GetHoleRectInOverlaySpace());
    }

    public void SetAllow(RectTransform target)
    {
        allow = target;
        if (isActiveAndEnabled) LateUpdate();
        gameObject.SetActive(true);
    }

    public void Hide() => gameObject.SetActive(false);

    void SetRectMode(RectTransform rt)
    {
        if (!rt) return;
        rt.anchorMin = Vector2.zero;   // 좌하 기준
        rt.anchorMax = Vector2.zero;
        rt.pivot     = Vector2.zero;
    }

    Rect GetHoleRectInOverlaySpace()
    {
        // allow의 월드 코너 -> 오버레이 로컬 좌표로 변환
        var wc = new Vector3[4];
        allow.GetWorldCorners(wc);

        Camera cam = overlayCanvas.renderMode == RenderMode.ScreenSpaceOverlay
                   ? null : overlayCanvas.worldCamera;

        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);

        for (int i = 0; i < 4; i++)
        {
            Vector2 sp = RectTransformUtility.WorldToScreenPoint(cam, wc[i]);
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                overlayRect, sp, cam, out var lp);
            min = Vector2.Min(min, lp);
            max = Vector2.Max(max, lp);
        }

        var r = Rect.MinMaxRect(min.x - padding, min.y - padding, max.x + padding, max.y + padding);
        return r;
    }

    void FitPanelsToMakeHole(Rect hole)
    {
        var R = overlayRect.rect; // overlay 로컬 좌표 (pivot이 0.5여도 ok)

        // 각 패널의 위치/크기(좌하 기준)
        // Bottom: 화면 아래 ~ hole 아래
        SetRect(bottomPanel, 0f,           0f,            R.width,             hole.yMin - R.yMin);
        // Top:    hole 위 ~ 화면 위
        SetRect(topPanel,    0f,           hole.yMax - R.yMin, R.width,         R.yMax - hole.yMax);
        // Left:   화면 좌 ~ hole 좌 (hole 높이만큼)
        SetRect(leftPanel,   0f,           hole.yMin - R.yMin, hole.xMin - R.xMin, hole.height);
        // Right:  hole 우 ~ 화면 우
        SetRect(rightPanel,  hole.xMax - R.xMin, hole.yMin - R.yMin,
                             R.xMax - hole.xMax,  hole.height);
    }

    void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        if (!rt) return;
        if (w < 0) w = 0; if (h < 0) h = 0;
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta        = new Vector2(w, h);
    }

    
}
