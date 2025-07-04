using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Ch1_Clock : BasePossessable
{
    private Image zoomPanel;
    private RectTransform startPos; // 두트윈 시작 위치
    private GameObject clockZoom; // 고해상도 시계 UI
    private Transform hourHand;
    private Transform minuteHand;
    private Ch1_TV  tvObject;
    
    private bool isControlMode = false;

    private int hour = 0;
    private int minute = 0;

    protected override void Start()
    {
        base.Start();

        // 확대UI 초기화
        zoomPanel = GameObject.Find("ZoomPanel").GetComponent<Image>();
        clockZoom = GameObject.Find("Ch1_ClockZoom");
        startPos = GameObject.Find("Ch1_ClockZoom").GetComponent<RectTransform>();
        hourHand = GameObject.Find("HourHand").transform;
        minuteHand = GameObject.Find("MinuteHand").transform;

        // UI 초기화
        clockZoom.SetActive(false); 

        // 시곗바늘 위치 초기화
        UpdateHands();
    }

    protected override void Update()
    {
        if (!isPossessed) return;

        // 최초 상호작용
        if (Input.GetKeyDown(KeyCode.E))
        {
            // 조작 종료
            isControlMode = false;
            HideClockUI();
            Unpossess();
        }

        if (!isControlMode) return;

        if (Input.GetKeyDown(KeyCode.A))
        {
            hour = (hour + 1) % 12;
            UpdateHands();
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            minute = (minute + 1) % 60;
            UpdateHands();
        }

        if (hour == 8 && minute == 14)
        {
            Debug.Log("정답");
            tvObject.ActivateTV();
            isControlMode = false;
            HideClockUI();
            hasActivated = false;
            Unpossess();
        }
    }

    private void UpdateHands()
    {
        if(hourHand != null)
            hourHand.localRotation = Quaternion.Euler(0, 0, -30f * hour);
        if (minuteHand != null)
            minuteHand.localRotation = Quaternion.Euler(0, 0, -6f * minute);
    }

    private void ShowClockUI()
    {
        //뒤에 어둡게 판넬 켜기
        zoomPanel.color = new Color(zoomPanel.color.r, zoomPanel.color.g, zoomPanel.color.b, 0f);
        zoomPanel.DOFade(150f / 255f, 0.5f);

        clockZoom.SetActive(true);
        startPos.anchoredPosition = new Vector2(0, -Screen.height); // 아래에서 시작
        startPos.DOAnchorPos(Vector2.zero, 0.5f).SetEase(Ease.OutCubic);
    }

    private void HideClockUI()
    {
        // 판넬 끄기
        zoomPanel.DOFade(0f, 0.5f);

        startPos.DOAnchorPos(new Vector2(0, -Screen.height), 0.5f)
            .SetEase(Ease.InCubic)
            .OnComplete(() =>
            {
                clockZoom.SetActive(false);
            });
    }

    public override void OnPossessionEnterComplete()
    {
        isControlMode = true;
        ShowClockUI();
    }
}
