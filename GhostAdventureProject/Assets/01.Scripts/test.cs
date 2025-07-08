using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Cinemachine;

public class test : MonoBehaviour
{


    public CinemachineVirtualCamera dd;

    private void Update()
    {

        if(Input.GetKeyDown(KeyCode.A))
        {
            SceneManager.LoadScene("Test");
            dd.Priority = 20;
        }

        if (Input.GetKeyDown(KeyCode.S))
        {
            SceneManager.LoadScene("Test");
            dd.Priority = 5;
        }
    }
}
