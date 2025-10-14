using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch04_TimeLine_CloseUI_StopPlayer : MonoBehaviour
{



    public void CloseUI_StopPlayer()
    {
        UIManager.Instance.PlayModeUI_CloseAll(); // 플레이모드 UI 닫기
        GameManager.Instance.Player.PossessionSystem.CanMove = false; // 플레이어 이동 비활성화
    }

    public void Reset_UI_Player()
    {
        UIManager.Instance.PlayModeUI_OpenAll(); // 플레이모드 UI 닫기
        GameManager.Instance.Player.PossessionSystem.CanMove = true; // 플레이어 이동 활성화
    }
}
 