using System.Collections.Generic;
using UnityEngine;

public class KeyBindingManager : MonoBehaviour
{
    public static KeyBindingManager Instance;

    public Dictionary<int, KeyCode> clueKeyBindings = new Dictionary<int, KeyCode>();

    private void Awake()
    {
        if (Instance == null) Instance = this;

        // 기본 키 설정 (Clue1~5)
        for (int i = 0; i < 5; i++)
        {
            clueKeyBindings[i] = KeyCode.Alpha1 + i;
        }
    }

    public void SetKey(int clueIndex, KeyCode key)
    {
        clueKeyBindings[clueIndex] = key;
    }

    public KeyCode GetKey(int clueIndex)
    {
        return clueKeyBindings.ContainsKey(clueIndex) ? clueKeyBindings[clueIndex] : KeyCode.None;
    }
}
