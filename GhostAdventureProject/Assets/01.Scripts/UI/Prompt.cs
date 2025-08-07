using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class Prompt : MonoBehaviour
{
    private GameObject PromptPanel; // 프롬프트 패널 이미지
    private TMP_Text PromptText; // 프롬프트 텍스트
    private Queue<string> PromptQueue = new Queue<string>();
    private bool isActive = false;
    private Tween promptTween;

    private void Start()
    {
        PromptPanel = gameObject;
        PromptText = gameObject.GetComponentInChildren<TextMeshProUGUI>();
        
        PromptPanel.SetActive(false);
    }


    // ===================== 대화용 - 클릭시 넘어감 ============================
    public void ShowPrompt_Click(params string[] lines)
    {
        PromptQueue.Clear();
        foreach (var line in lines)
            PromptQueue.Enqueue(line);

        PromptPanel.SetActive(true);
        enabled = true; // Update 실행 가능하게
        ShowNextLine();
    }

    private void ShowNextLine()
    {
        if (PromptQueue.Count > 0)
        {
            string nextLine = PromptQueue.Dequeue();
            PromptText.text = nextLine;
        }
        else
        {
            PromptPanel.SetActive(false);
            enabled = false; // Update 비활성화
        }
    }

    // private void Update()
    // {
    //     if (Input.GetMouseButtonDown(0))
    //         ShowNextLine();
    // }
    // public void ShowPrompt(List<string> lines) //, System.Action onComplete = null
    // {
    //     PromptQueue.Clear();
    //     foreach (var line in lines)
    //         PromptQueue.Enqueue(line);

    //     // onDialogComplete = onComplete;
    //     PromptPanel.SetActive(true);
    //     isActive = true;
    //     ShowNextLine();
    // }

    // private void ShowNextLine()
    // {
    //     if (PromptQueue.Count > 0)
    //     {
    //         string nextLine = PromptQueue.Dequeue();
    //         PromptText.text = nextLine;
    //     }
    //     else
    //     {
    //         PromptPanel.SetActive(false);
    //         isActive = false;
    //         // onDialogComplete?.Invoke();
    //     }
    // }
    

    // private void Update()
    // {
    //     if(!isActive) return;
        
    //     if(Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
    //         ShowNextLine();
    // }

    // ===================== 알림용 - 일정시간 후 자동사라짐 ============================


    // 메시지와 시간 둘 다 받음
    public void ShowPrompt(string line, float delaytime)
    {
        PromptPanel.SetActive(true); // 패널 보이게하기
        PromptText.text = line;
        
        promptTween?.Kill();

        promptTween = DOVirtual.DelayedCall(delaytime, () =>
        {
            PromptPanel.SetActive(false);
            isActive = false;
        });
        // StartCoroutine(HideAfterDelay(delaytime));
    }


    // private IEnumerator HideAfterDelay(float dlaytime)
    // {
    //     yield return new WaitForSecondsRealtime(dlaytime);
    //     PromptPanel.SetActive(false);
    //     isActive = false;
    //     // onD
    // }

    // 기본프롬프트 2초 ==============================================================
    public void ShowPrompt(string line)
    {
        PromptPanel.SetActive(true); // 패널 보이게하기
        PromptText.text = line;

        promptTween?.Kill();

        promptTween = DOVirtual.DelayedCall(2f, () =>
        {
            PromptPanel.SetActive(false);
            isActive = false;
        });
        
        // StartCoroutine(HideAfterDelay());
    }


    // private IEnumerator HideAfterDelay()
    // {
    //     yield return new WaitForSecondsRealtime(2f);
    //     PromptPanel.SetActive(false);
    //     isActive = false;
    //     // onD
    // }
    // ==============================================================================

    public void ShowPrompt_2 (params string[] lines)
    {
        PromptPanel.SetActive(true);
        isActive = true;

        promptTween?.Kill();

        Sequence seq = DOTween.Sequence();

        foreach (string line in lines)
        {
            seq.AppendCallback(() => 
            {
                PromptText.text = line;
            });
            seq.AppendInterval(1.5f);
        }
         seq.AppendCallback(() =>
        {
            PromptPanel.SetActive(false);
            isActive = false;
        });
        promptTween = seq;
        // StartCoroutine(ShowPromptSequence(lines));
    }

    // 여러 대사 중 랜덤 출력 ========================================================================
    public void ShowPrompt_Random(params string[] lines)
    {
        if (lines == null || lines.Length == 0) return;

        PromptPanel.SetActive(true);
        isActive = true;

        promptTween?.Kill();

        Sequence seq = DOTween.Sequence();

        // ✅ 랜덤으로 한 줄 선택
        string chosenLine = lines[Random.Range(0, lines.Length)];

        seq.AppendCallback(() => 
        {
            PromptText.text = chosenLine;
        });
        seq.AppendInterval(1.5f);

        seq.AppendCallback(() =>
        {
            PromptPanel.SetActive(false);
            isActive = false;
        });

        promptTween = seq;
    }

}


