using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch04_Ending_Choice : MonoBehaviour
{

    [SerializeField ]GameObject canvas;

    private void Awake()
    {

        canvas = GetComponent<GameObject>();
        if (canvas == null)
            Debug.LogError("Null");
    }
    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            canvas.SetActive(false);

        }
    }
}

