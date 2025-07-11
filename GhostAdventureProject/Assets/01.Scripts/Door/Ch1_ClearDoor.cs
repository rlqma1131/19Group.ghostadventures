using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
public class Ch1_ClearDoor : MonoBehaviour
{
    [SerializeField] private GameObject TeddyBear;
    public Ch1_MemoryPositive_01_TeddyBear teddyBearScript;
    private bool canOpenDoor = false;

    void Start()
    {
     teddyBearScript=TeddyBear.GetComponent<Ch1_MemoryPositive_01_TeddyBear>();

        Debug.Log(teddyBearScript.Completed_TeddyBear+"dddddddddddddddddddddddddddddddd");
    }

    private void Update()
    {

        if (Input.GetKeyDown(KeyCode.E))
        {
        if (teddyBearScript.Completed_TeddyBear)
        {
            // 플레이어가 문에 접근했을 때, 문을 열고 다음 씬으로 이동
            Debug.Log("문이 열렸습니다. 다음 씬으로 이동합니다.");
            // 다음 씬으로 이동하는 코드 작성
            SceneManager.LoadScene("Ch02");
        }
        else
        {
            Debug.Log("테디베어를 먼저 수집해야 합니다.");
        }
            
        }
    }
    // Update is called once per frame
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            canOpenDoor = true;

        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {


            canOpenDoor = false;
        }
            


        

    }
}

