using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YameDoor : MonoBehaviour
{
    private CH2_SecurityGuard guard;
    [SerializeField] private GameObject e_key;
    [SerializeField] private GameObject targetDoor;
    private bool moveAble = false;


    void Start()
    {
        e_key.SetActive(false);    
    }

    void Update()
    {
        if(moveAble)
        {
            if(Input.GetKeyDown(KeyCode.E))
            {
                Vector3 targetDoorPos = guard.transform.position;
                targetDoorPos.x = targetDoor.transform.position.x;   
                guard.transform.position = targetDoorPos;
            }
        }
        
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Person"))
        {
            guard = collision.GetComponent<CH2_SecurityGuard>();

            if (guard != null && guard.isPossessed)
            {
                guard.doorPass = true;
                e_key.SetActive(true);
                moveAble = true;
                Debug.Log("관리인 야매문과 충돌");
            }
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if (collision != null && collision.CompareTag("Person"))
        {
            guard = collision.GetComponent<CH2_SecurityGuard>();

            if (guard != null && guard.isPossessed)
            {
                guard.doorPass = false;
                e_key.SetActive(false);
                moveAble = false;
                Debug.Log("관리인 야매문과 충돌");
            }
        }
    }
}
