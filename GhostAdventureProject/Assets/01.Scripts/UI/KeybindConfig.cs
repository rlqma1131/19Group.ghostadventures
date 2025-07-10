using System.Collections;
using System.Collections.Generic;
using UnityEngine;


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
}
