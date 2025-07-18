using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class EnemyVolumeTrigger : MonoBehaviour
{
   [SerializeField] private Volume globalVolume;
    void Start()
    {
        globalVolume = GetComponentInChildren<Volume>();
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (globalVolume != null)
            {
                globalVolume.enabled = true;
            }
        }
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            if (globalVolume != null)
            {
                globalVolume.enabled = false;
            }
        }
    }


}
