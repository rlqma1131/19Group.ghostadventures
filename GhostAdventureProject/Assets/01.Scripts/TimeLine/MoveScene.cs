using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveScene : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if(Input.GetKeyDown(KeyCode.S))
        {
            GoScene("TestScene"); //S키를 누르거나 마우스 클릭시 메인 씬으로 이동
        }

    }

    public void GoScene(string Scenename)
    {
        // 타임라인이 종료되면 씬 이동
        SceneManager.LoadScene(Scenename);
    }
}
