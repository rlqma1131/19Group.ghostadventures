using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBoard_Enter : MonoBehaviour
{
    public TextMeshProUGUI outputText;         // 유저가 입력한 문자열
    public string correctAnswer;    // 정답

    void Start()
    {
        GetComponent<Button>().onClick.AddListener(OnEnterClick);
    }
    
    void OnEnterClick()
    {
        CheckAnswer();
    }
    
    // 정답 확인
    public void CheckAnswer()
    {
        if (outputText == null) return;

        string userInput = outputText.text;

        if (userInput.Equals(correctAnswer, System.StringComparison.OrdinalIgnoreCase)) 
        {
            Debug.Log("정답입니다!");
            // 여기서 정답 처리 (이펙트, 다음 단계 등)
        }
        else
        {
            Debug.Log("틀렸습니다.");
            ClearOutput();
            // 틀렸을 때의 처리 (진동, 리셋 등)
        }
    }

    public void ClearOutput()
    {
        if (outputText != null)
            outputText.text = "";
    }

}
