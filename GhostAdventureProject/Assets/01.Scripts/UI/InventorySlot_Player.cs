using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot_Player : MonoBehaviour
{
    public Image icon;
    public Image clearSlotIcon;
    public int clueIndex;
    public TMP_Text keyText;

    // public TextMeshProUGUI clueName;

    public void Setup(ClueData clue)
    {
        icon.sprite = clue.clue_Icon;
        icon.enabled = true; // 아이콘 표시
        // clueName.text = clue.clue_Name;
    }

    internal void Clear()
    {
        icon.sprite = null;
        icon.enabled = false; // 아이콘 숨기기
    }


    private void Start()
    {
        UpdateKeyText();
        icon.enabled = false;
    }

    public void UpdateKeyText()
    {
        if (keyText == null || UIManager.Instance.ESCMenuUI == null) return;
        KeyCode key = UIManager.Instance.ESCMenuUI.GetKey(clueIndex);
        keyText.text = ESCMenu.KeyNameHelper.GetDisplayName(key);
    }
}
