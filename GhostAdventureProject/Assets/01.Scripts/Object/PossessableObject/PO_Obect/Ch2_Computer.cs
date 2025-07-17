using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using TMPro;
using UnityEngine.UI;

public class Ch2_Computer : BasePossessable
{
    [Header("UI References")]
    [SerializeField] private RectTransform monitorPanel;
    [SerializeField] private GameObject fileIcon;
    [SerializeField] private GameObject passwordPanel;
    [SerializeField] private TMP_InputField passwordInput;
    [SerializeField] private Button confirmButton;

    [Header("Puzzle Settings")]
    [SerializeField] private string correctPassword;
    [SerializeField] private GameObject doorToOpen;
    [SerializeField] private GameObject monitorOn;
    
    [SerializeField] private GameObject q_Key;

    private Vector2 hiddenPos = new(0, -800);
    private Vector2 visiblePos = new(0, 0);

    private bool isPanelOpen = false;
    private float lastClickTime;
    private const float doubleClickDelay = 0.3f;

    protected override void Start()
    {
        hasActivated = false;
        monitorPanel.anchoredPosition = hiddenPos;
        monitorPanel.gameObject.SetActive(false);
        passwordPanel.SetActive(false);
        
        if (confirmButton != null)
        {
            confirmButton.onClick.AddListener(SubmitPassword);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed || !hasActivated)
        {
            q_Key.SetActive(false);
            return;
        }

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (!isPanelOpen)
            {
                q_Key.SetActive(false);
                OpenPanel();
            }
            else
            {
                q_Key.SetActive(true);
                ClosePanel();
            }
        }

        if (isPanelOpen && Input.GetKeyDown(KeyCode.E))
        {
            Debug.Log("패널 열려있음 - 빙의 해제 차단됨");
            return;
        }

        // 엔터 키로 비밀번호 제출
        if (passwordPanel.activeSelf && Input.GetKeyDown(KeyCode.Return))
        {
            SubmitPassword();
        }
        
        q_Key.SetActive(true);
    }

    public void OpenPanel()
    {
        isPanelOpen = true;
        monitorPanel.gameObject.SetActive(true);
        monitorPanel.DOAnchorPos(visiblePos, 0.5f).SetEase(Ease.OutBack);
    }

    public void ClosePanel()
    {
        isPanelOpen = false;
        passwordPanel.SetActive(false);
        monitorPanel.DOAnchorPos(hiddenPos, 0.5f)
            .SetEase(Ease.InBack)
            .OnComplete(() =>
            {
                monitorPanel.gameObject.SetActive(false);
            });
    }

    // 파일 아이콘 클릭 → 더블클릭 체크
    public void OnFileIconClick()
    {
        float time = Time.time;
        if (time - lastClickTime < doubleClickDelay)
        {
            OpenPasswordPanel();
        }
        lastClickTime = time;
    }

    private void OpenPasswordPanel()
    {
        passwordPanel.SetActive(true);
        passwordInput.text = "";
        passwordInput.ActivateInputField();
    }

    private void SubmitPassword()
    {
        string input = passwordInput.text.Trim();

        if (input == correctPassword)
        {
            Debug.Log(" 비밀번호 정답 ");
            doorToOpen.SetActive(true);
            ClosePanel();
        }
        else
        {
            Debug.Log(" 비밀번호 틀림 ");
            passwordInput.text = "";
            passwordInput.ActivateInputField();
        }
    }

    public void Activate()
    {
        hasActivated = true;
        monitorOn.SetActive(false);
    }
}
