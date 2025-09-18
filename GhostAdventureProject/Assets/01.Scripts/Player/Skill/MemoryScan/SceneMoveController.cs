using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SceneMoveController : MonoBehaviour
{
    void Scenemove()
    {

        // 현재 씬의 이름을 가져옵니다.
        string currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name;
        // 다음 씬의 이름을 결정합니다.
        string nextSceneName = currentSceneName == "Scene1" ? "Scene2" : "Scene1";
        // 다음 씬으로 이동합니다.
        UnityEngine.SceneManagement.SceneManager.LoadScene(nextSceneName);
    }
}
