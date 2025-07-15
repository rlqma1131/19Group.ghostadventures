using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


[System.Serializable]
public class ClueKeyBinding
{
    public int slotIndex; // 0~4
    public KeyCode key;
}

[CreateAssetMenu(menuName = "Config/KeybindConfig")]
public class KeybindConfig : ScriptableObject
{
    public List<ClueKeyBinding> clueKeyBindings;
    [SerializeField] private List<Button> rebindButtons; // 슬롯 0~4 순서대로 연결
    [SerializeField] private List<Text> buttonLabels;    // 버튼 안에 있는 Text 컴포넌트

    public KeyCode GetKeyForSlot(int slotIndex)
    {
        foreach (var binding in clueKeyBindings)
        {
            if (binding.slotIndex == slotIndex)
                return binding.key;
        }
        return KeyCode.None;
    }

    public void SetKeyForSlot(int slotIndex, KeyCode newKey)
    {
        foreach (var binding in clueKeyBindings)
        {
            if (binding.slotIndex == slotIndex)
            {
                binding.key = newKey;
                return;
            }
        }
    }

    // ========================================================================================
      public class ClueKeyBindingUI : MonoBehaviour
    {
        [SerializeField] private KeybindConfig keybindConfig;
        [SerializeField] private List<Button> rebindButtons; // 슬롯 0~4 순서대로 연결
        [SerializeField] private List<Text> buttonLabels;    // 버튼 안에 있는 Text 컴포넌트
        private int waitingSlot = -1;

        private void Start()
        {
            LoadKeyBindings();

            // 각 버튼에 이벤트 연결
            for (int i = 0; i < rebindButtons.Count; i++)
            {
                int index = i;
                rebindButtons[i].onClick.AddListener(() => BeginRebind(index));
            }

            UpdateLabels();
        }

        private void Update()
        {
            if (waitingSlot != -1)
            {
                foreach (KeyCode key in System.Enum.GetValues(typeof(KeyCode)))
                {
                    if (Input.GetKeyDown(key))
                    {
                        keybindConfig.SetKeyForSlot(waitingSlot, key);
                        PlayerPrefs.SetString($"ClueKey{waitingSlot}", key.ToString());
                        Debug.Log($"Clue {waitingSlot + 1} key set to {key}");
                        waitingSlot = -1;
                        UpdateLabels();
                        break;
                    }
                }
            }
        }

        private void BeginRebind(int slotIndex)
        {
            waitingSlot = slotIndex;
            buttonLabels[slotIndex].text = $"Clue {slotIndex + 1} : [Press key...]";
        }

        private void UpdateLabels()
        {
            for (int i = 0; i < buttonLabels.Count; i++)
            {
                KeyCode key = keybindConfig.GetKeyForSlot(i);
                buttonLabels[i].text = $"Clue {i + 1} : [{key}]";
            }
        }

        private void LoadKeyBindings()
        {
            for (int i = 0; i < 5; i++)
            {
                string saved = PlayerPrefs.GetString($"ClueKey{i}", $"Alpha{i + 1}");
                if (System.Enum.TryParse(saved, out KeyCode key))
                {
                    keybindConfig.SetKeyForSlot(i, key);
                }
            }
        }
    }
}
