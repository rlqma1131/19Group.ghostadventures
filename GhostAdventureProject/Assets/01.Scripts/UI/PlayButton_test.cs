using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayButton_test : MonoBehaviour
{
    public void PlayButton()
    {
        SceneManager.LoadScene("TestScene");
        UIManager.Instance.ShowAll();
    }
}
