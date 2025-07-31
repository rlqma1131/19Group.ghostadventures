using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DoorIconTrigger : MonoBehaviour
{
    [SerializeField] private GameObject Icon;

    void Start()
    {
        Icon.SetActive(false);
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Icon.SetActive(true);
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            Icon.SetActive(false);
        }
        
    }
}
