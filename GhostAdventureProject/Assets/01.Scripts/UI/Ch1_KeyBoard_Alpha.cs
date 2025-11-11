using UnityEngine;
using TMPro;
using UnityEngine.UI;


public class Ch1_KeyBoard_Alpha : MonoBehaviour
{
    public string keyValue; // 버튼이 나타내는 문자
    public Ch1_KeyBoard textSlot;

    void Start()
    {   
        keyValue = GetComponentInChildren<TextMeshProUGUI>().text;
        GetComponent<Button>().onClick.AddListener(OnKeyClick);
    }

    void OnKeyClick()
    {
        textSlot?.AddLetter(keyValue);
    }
}


