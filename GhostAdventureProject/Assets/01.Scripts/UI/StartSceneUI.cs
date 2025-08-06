using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class StartSceneUI : MonoBehaviour
{
    private Text surveyText;

    private void Start()
    {

    }
    // Start is called before the first frame update
    public void introScenestart()
    {
        Debug.Log("Play버튼 클릭");
        SceneManager.LoadScene("IntroScene_Real");
        if(UIManager.Instance != null)
        {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }

    }

    public void Ch02Start()
    {

        Debug.Log("Ch02Start버튼 클릭");
        SceneManager.LoadScene("Ch01_To_Ch02");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }

    public void Ch03Start()
    {

        Debug.Log("Ch03Start버튼 클릭");
        SceneManager.LoadScene("Ch02_To_Ch03");
        if (UIManager.Instance != null)
        {
            UIManager.Instance.PlayModeUI_CloseAll();
            UIManager.Instance.startEndingUI_CloseAll();
        }
    }
    public void OpenURL()
    {
        surveyText = GetComponentInChildren<Text>();
        Application.OpenURL("https://docs.google.com/forms/d/e/1FAIpQLSetE6cy2Iu6odXTSfW-ym8_2uxIw4b539wSyZo0Io8N3jNoeg/viewform?usp=dialog");
        surveyText.text = "감사합니다!";
    }
    public void ExitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
