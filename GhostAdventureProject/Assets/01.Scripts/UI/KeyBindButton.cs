using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class KeyBindButton : MonoBehaviour
{
    public int clueIndex;
    private Button button;
    public Text keyText;

    private bool isWaitingForKey = false;

    private void Start()
    {
        button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
        UpdateKeyText();
    }

    void OnClick()
    {
        isWaitingForKey = true;
        keyText.text = "Press key...";
    }

    private void Update()
    {
        if (isWaitingForKey && Input.anyKeyDown)
        {
            foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
            {
                if (Input.GetKeyDown(key))
                {
                    KeyBindingManager.Instance.SetKey(clueIndex, key);
                    isWaitingForKey = false;
                    UpdateKeyText();
                    NotifyInventoryToUpdateKey();
                    break;
                }
            }
        }
    }

    void UpdateKeyText()
    {
        keyText.text = KeyBindingManager.Instance.GetKey(clueIndex).ToString();
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
