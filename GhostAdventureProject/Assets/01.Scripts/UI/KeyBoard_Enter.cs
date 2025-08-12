using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBoard_Enter : MonoBehaviour
{
    public TextMeshProUGUI outputText;  // 유저가 입력한 문자
    public KeyBoard textSlot;
    public string correctAnswer;    // 정답
    public bool correct = false;

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
            correct = true;
            textSlot.Close(); // 키보드 닫기
            PossessionSystem.Instance.CanMove = true;
            Debug.Log("정답!");
        }
        else
        {
            correct = false;
            PossessionSystem.Instance.CanMove = true;
            UIManager.Instance.PromptUI.ShowPrompt("순서가...기억을 되짚어 보자");
        }
        textSlot.ClearAll();
    }
}
