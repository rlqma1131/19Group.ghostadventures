using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;

public class Ch2_Kiosk : BasePossessable
{
    [SerializeField] private GameObject q_Key;
    [SerializeField] private RectTransform kioskPanel;
    [SerializeField] private GameObject hintNoteObject;
    [SerializeField] private GameObject hiddenDoorObject;
    
    [SerializeField] private Button[] allButtons;
    [SerializeField] private List<Button> correctSequence; // 인스펙터에서 정답 순서대로 등록
    [SerializeField] private List<Button> hiddenSequence;  // 히든 정답, 숨은 통로
    [SerializeField] private Button confirmButton;
    [SerializeField] private TextMeshProUGUI inputTextUI;
    
    private List<Button> currentInput = new();
    private bool isPanelOpen = false;
    private Vector2 hiddenPos = new(0, -800);
    private Vector2 visiblePos = new(0, 0);

    protected override void Start()
    {
        hasActivated = false;
        kioskPanel.anchoredPosition = hiddenPos;
        kioskPanel.gameObject.SetActive(false);
    }

    protected override void Update()
    {
        // base.Update();

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
                ClosePanel();
            }
        }

        if (!isPanelOpen && Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
        }
        
        q_Key.SetActive(true);
    }

    public void Activate()
    {
        hasActivated = true;
    }
    
    private void OpenPanel()
    {
        isPanelOpen = true;
        kioskPanel.gameObject.SetActive(true);
        kioskPanel.DOAnchorPos(visiblePos, 0.5f).SetEase(Ease.OutBack);

        currentInput.Clear();
        
        // 버튼 이벤트 연결
        foreach (var btn in allButtons)
        {
            btn.onClick.RemoveAllListeners();
            btn.onClick.AddListener(() => OnButtonPressed(btn));
        }
        
        confirmButton.onClick.RemoveAllListeners();
        confirmButton.onClick.AddListener(CheckAnswer);
    }
    
    private void ClosePanel()
    {
        isPanelOpen = false;

        kioskPanel.DOAnchorPos(hiddenPos, 0.5f)
                  .SetEase(Ease.InBack)
                  .OnComplete(() =>
                  {
                      kioskPanel.gameObject.SetActive(false);
                  });

        currentInput.Clear();
        UpdateInputDisplay();
    }

    private void OnButtonPressed(Button btn)
    {
        currentInput.Add(btn);
        Debug.Log($"버튼 입력됨: {btn.name}");
        UpdateInputDisplay();
    }

    private void CheckAnswer()
    {
        bool isCorrect = CompareSequence(currentInput, correctSequence);
        bool isHidden = CompareSequence(currentInput, hiddenSequence);

        if (isCorrect)
        {
            Debug.Log(" 정답 ");
            hintNoteObject.SetActive(true);
            ClosePanel();
        }
        else if (isHidden)
        {
            Debug.Log(" 히든 정답 ");
            hiddenDoorObject.SetActive(true);
            ClosePanel();
        }
        else
        {
            Debug.Log(" 오답 ");
            currentInput.Clear();
            UpdateInputDisplay();
        }
    }

    private bool CompareSequence(List<Button> input, List<Button> target)
    {
        if (input.Count != target.Count) return false;

        for (int i = 0; i < target.Count; i++)
        {
            if (input[i] != target[i]) return false;
        }

        return true;
    }

    private void UpdateInputDisplay()
    {
        string sequence = string.Join(" > ", currentInput.Select(b => b.name));
        inputTextUI.text = sequence;
    }
}