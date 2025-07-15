using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StartSceneUI : MonoBehaviour
{
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

    public void ExitGameButton()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }

}
