using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class teststartScene : MonoBehaviour
{
    // Start is called before the first frame update
    public void introScenestart()
    {
        Debug.Log("Play버튼 클릭");
        SceneManager.LoadScene("IntroScene_Real");
    }

    public void ExitGameButton()
    {
        // Application.Quit();
        Debug.Log("게임이 종료되었습니다");
    }

}
