using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MirrorRoomExitTrigger : MonoBehaviour
{
    private bool hasTriggered = false;
    [SerializeField] private GameObject mirrorRoomExit;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (hasTriggered) return;
        if (!other.CompareTag("Player")) return;
        
        hasTriggered = true;
        mirrorRoomExit.SetActive(true);
    }
}
