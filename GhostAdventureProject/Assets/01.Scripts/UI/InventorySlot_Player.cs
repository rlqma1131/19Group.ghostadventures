using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class InventorySlot_Player : MonoBehaviour
{
    public Image icon;
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
}
