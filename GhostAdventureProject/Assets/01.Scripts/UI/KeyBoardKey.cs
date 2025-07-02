using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class KeyBoardKey : MonoBehaviour
{
    public string keyValue; // 버튼이 나타내는 문자
    public TMP_Text outputText; // 연결된 텍스트

    void Start()
    {
        keyValue = GetComponentInChildren<TextMeshProUGUI>().text;
        GetComponent<Button>().onClick.AddListener(OnKeyClick);
    }

    void OnKeyClick()
    {
        if (outputText != null)
        {
            outputText.text += keyValue;
        }
    }
}


