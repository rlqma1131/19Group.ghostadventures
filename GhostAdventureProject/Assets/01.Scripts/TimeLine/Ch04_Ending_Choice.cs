using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch04_Ending_Choice : MonoBehaviour
{

    [SerializeField ]GameObject canvas;

    private void Awake()
    {

       // canvas = GetComponent<GameObject>();
        if (canvas == null)
            Debug.LogError("Null");
    }

    //플레이어가 2층 벗어나면 맵 패널 비활성화

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (canvas != null && !ReferenceEquals(canvas, null) && canvas.activeSelf)
            {
                canvas.SetActive(false);
            }
        }
    }

}

