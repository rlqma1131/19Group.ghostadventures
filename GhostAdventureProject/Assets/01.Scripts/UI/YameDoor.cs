using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YameDoor : MonoBehaviour
{
    [SerializeField] private CH2_SecurityGuard guard;
    // [SerializeField] private GameObject e_key;
    [SerializeField] private GameObject targetDoor;
    [SerializeField] private GameObject closeSprite;
    [SerializeField] private GameObject player;

    private bool moveAble = false;


     void Start()
    {
        player = FindObjectOfType<PlayerController>().gameObject;
        // e_key.SetActive(false);    
    }


    void Update()
    {
        // if(guard.doorPass && guard.isdoorLockOpen)
        // {
        //     if(Input.GetKeyDown(KeyCode.E))
        //     {
        //         if(guard.isPossessed)
        //         { 
        //             Vector3 targetDoorPos = guard.transform.position;
        //             targetDoorPos.x = targetDoor.transform.position.x;   
        //             guard.transform.position = targetDoorPos;
        //         }
        //         if(!guard.isPossessed)
        //         {
        //             Vector3 targetDoorPos = player.transform.position;
        //             targetDoorPos.x = targetDoor.transform.position.x;   
        //             player.transform.position = targetDoorPos;

        //         }
        //     }
        // }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            // if (guard != null && guard.isPossessed)
            // {
                guard.doorPass = true;
                // e_key.SetActive(true);
                moveAble = true;
                Debug.Log("관리인 야매문과 충돌");
            // }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Person") || collision.CompareTag("Player"))
        {
            // if (guard != null && guard.isPossessed)
            // {
                guard.doorPass = false;
                // e_key.SetActive(false);
                moveAble = false;
                Debug.Log("관리인 야매문과 충돌해제");
            // }
        }
    }
}