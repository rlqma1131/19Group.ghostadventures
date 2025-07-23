using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class Prompt_UnPlayMode : MonoBehaviour
{
    private GameObject PromptPanel; // 프롬프트 패널 이미지
    private TMP_Text PromptText; // 프롬프트 텍스트
    private Queue<string> PromptQueue = new Queue<string>();
    private System.Action onDialogComplete;
    private bool isActive = false;

    private void Start() 
    {
        PromptPanel = gameObject;
        PromptText = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        PromptPanel.SetActive(false);
    }


    // ===================== 대화용 - 클릭시 넘어감 ============================

    public void ShowPrompt_UnPlayMode(string[] lines) //, System.Action onComplete = null
    {
        PromptQueue.Clear();
        foreach (var line in lines)
            PromptQueue.Enqueue(line);

        // onDialogComplete = onComplete;
        PromptPanel.SetActive(true);
        isActive = true;
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
            isActive = false;
            // onDialogComplete?.Invoke();
        }
    }


    private void Update()
    {
        if (!isActive) return;

        if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.F))
            ShowNextLine();
    }

    // ===================== 알림용 - 일정시간 후 자동사라짐 ============================


    // 메시지와 시간 둘 다 받음
    public void ShowPrompt_UnPlayMode(string line, float dlaytime)
    {

        PromptPanel.SetActive(true); // 패널 보이게하기
        PromptText.text = line;
        StartCoroutine(HideAfterDelay(dlaytime));
    }


    private IEnumerator HideAfterDelay(float dlaytime)
    {
        yield return new WaitForSecondsRealtime(dlaytime);
        PromptPanel.SetActive(false);
        isActive = false;
        // onD
    }
}


