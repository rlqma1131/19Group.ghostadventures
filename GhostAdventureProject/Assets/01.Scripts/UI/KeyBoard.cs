using System;
using System.Collections;
using System.Collections.Generic;
using _01.Scripts.Player;
using UnityEngine;
using TMPro;

public class KeyBoard : MonoBehaviour, IUIClosable
{
    [SerializeField] EnergyRestoreZone energyRestoreZone;
    public TMP_Text[] letterSlots = new TMP_Text[4];
    private int currentIndex = 0;
    public GameObject keyBoardPanel;

    Player player;

    void Start() {
        player = GameManager.Instance.Player;
    }

    public void AddLetter(string letter)
    {
        if (currentIndex < letterSlots.Length)
        {
            letterSlots[currentIndex].text = letter;
            currentIndex++;
        }
    }

    // 글자 한개만 지우기
    public void DeleteLastLetter()
    {
        if (currentIndex > 0)
        {
            currentIndex--;
            letterSlots[currentIndex].text = "_";
        }
    }

    // 입력된 4개 글자 확인
    public string GetCurrentWord()
    {
        string word = "";
        foreach (var slot in letterSlots)
        {
            word += slot.text != "_" ? slot.text : "";
        }
        return word;
    }

    // 글자 모두 지우기
    public void ClearAll()
    {
        currentIndex = 0;
        foreach (var slot in letterSlots)
        {
            slot.text = "_";
        }
    }

    // 키보드UI 열기
    public void OpenKeyBoard()
    {
        EnemyAI.PauseAllEnemies();

        UIManager.Instance.NoticePopupUI.FadeInAndOut("※ 수집한 단서로 이름을 입력하세요.");
        keyBoardPanel.SetActive(true); // 키보드UI 열기
        UIManager.Instance.PlayModeUI_CloseAll();
    }
    
    // 키보드UI 닫기
    public void Close()
    {
        SoulEnergySystem.Instance.EnableHealingEffect();
        EnemyAI.ResumeAllEnemies();

        keyBoardPanel.SetActive(false);
        UIManager.Instance.PlayModeUI_OpenAll();
        player.PossessionSystem.CanMove = true;
    }

    public bool IsOpen()
    {
        return this.gameObject.activeInHierarchy;
    }
}
