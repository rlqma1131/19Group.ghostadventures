using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBoard_Enter : MonoBehaviour
{
    public TextMeshProUGUI outputText;  // 유저가 입력한 문자
    public string correctAnswer;    // 정답
    public KeyBoard textSlot;

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
          string input = textSlot.GetCurrentWord();
            if (input == correctAnswer)
            {
                Debug.Log("정답!");
            }
            else
            {
                Debug.Log("오답!");
            }
            textSlot.ClearAll();
    }

}
