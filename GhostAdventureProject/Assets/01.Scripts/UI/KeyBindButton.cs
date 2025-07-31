using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindButton : MonoBehaviour
{
    public int clueIndex;
    private Button button;
    public TextMeshProUGUI keyText;

    private bool isWaitingForKey = false;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        // UpdateKeyText();
    }

    void OnClick()
    {
        isWaitingForKey = true;
        keyText.text = "PressKey";
    }

    private void Update()
    {
        if (isWaitingForKey && Input.anyKeyDown)
        {
        foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
        {
            if (Input.GetKeyDown(key))
            {
                // 숫자 키 (Alpha1 ~ Alpha9) 또는 F키 (F1 ~ F9)만 허용
                bool isAlphaKey = key >= KeyCode.Alpha1 && key <= KeyCode.Alpha9;
                bool isFKey = key >= KeyCode.F1 && key <= KeyCode.F9;

                if (isAlphaKey || isFKey)
                {
                    UIManager.Instance.ESCMenuUI.SetKey(clueIndex, key);
                    isWaitingForKey = false;
                    UpdateKeyText();
                    NotifyInventoryToUpdateKey();
                }
                else
                {
                    Debug.Log("숫자 키 또는 F1~F9 키만 사용할 수 있습니다.");
                }

                break;
            }
                
        }
        }




    }

    void UpdateKeyText()
    {
        string keyName = UIManager.Instance.ESCMenuUI.GetKey(clueIndex).ToString();        
        keyText.text = keyName.Replace("Alpha", "");
    }


    private void NotifyInventoryToUpdateKey()
{
    // 모든 ClueSlotUI 찾아서 해당 인덱스만 갱신
    InventorySlot_Player[] slots = FindObjectsOfType<InventorySlot_Player>();
    foreach (var slot in slots)
    {
        if (slot.clueIndex == clueIndex)
        {
            slot.UpdateKeyText();
        }
    }
}
}
