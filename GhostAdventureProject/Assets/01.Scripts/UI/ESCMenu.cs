using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ESCMenu : MonoBehaviour, IUIClosable
{
    //esc키를 누르면 또는 버튼을 누르면
    //ESCMenu UI가 뜸
    //이어하기, 타이틀로, 설정, 종료 버튼이 있음
    //설정버튼을 누르면 사운드/언어/조작키설정 이 있음

    //이어하기 버튼을 누르면 멈췄던 게임이 다시 진행됨
    //타이틀로 버튼을 누르면 start화면으로 감
    //설정을 누르면 하위 메뉴가 뜸
    //종료버튼을 누르면 게임 종료됨.


    //=======================

    [SerializeField] private GameObject escMenuUI; // ESCMenuCanvas
    [SerializeField] private SettingMenu settingsMenu;
    private bool isPaused = false;

    void Start()
    {
        escMenuUI.SetActive(false);
    }

    // ESC메뉴 열기/닫기
    public void ESCMenuToggle()
    {
        if (isPaused)
        {
            ResumeGame();
        }
        else
        {
            ESCMenu_Open();
        }
        
    }
    public void ESCMenu_Open()
    {
        escMenuUI.SetActive(true);
        Time.timeScale = 0f;
        isPaused = true;
    }
    
    public void Close()
    {
        escMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;      
    }

    public bool IsOpen()
    {
        return UIManager.Instance.ESCMenuUI.gameObject.activeInHierarchy;
    }


    // 이어하기
    public void ResumeGame()
    {
        escMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;   

    }

    // 타이틀로
    public void GoToMainMenu()
    {
        UIManager.Instance.PlayModeUI_CloseAll();
        escMenuUI.SetActive(false);
        Time.timeScale = 1f;
        isPaused = false;
        PossessionSystem.Instance.CanMove = true; // 임시완

        if (PlayerLifeManager.Instance != null) // 플레이어 생명 불러오기 
        {
            PlayerLifeManager.Instance.ResetLives();
        }

        SceneManager.LoadScene("StartScene");
    }

    // 설정
    public void SettingButton()
    {
        settingsMenu.OpenSettings();
    }

    // 게임종료
    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
