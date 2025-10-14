using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
public class Ch04_endingYesOrNO : MonoBehaviour
{


    [SerializeField] private GameObject Panel;
    //[SerializeField] private Button YesButton;
   // [SerializeField] private Button NoButton;
    //public string SceneName;
    private void Start()
    {
         Panel.SetActive(false);
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {   Time.timeScale = 0f; // 게임 일시정지
            Panel.SetActive(true);
        }
    }


    public void NoButtonAction()
    {
        Time.timeScale = 1f; // 게임 재개
        Panel.SetActive(false);
    }

    public void YesButtonAction(string SceneName)
    {
        Time.timeScale = 1f; // 게임 재개
        SceneManager.LoadScene(SceneName);
    }
}
