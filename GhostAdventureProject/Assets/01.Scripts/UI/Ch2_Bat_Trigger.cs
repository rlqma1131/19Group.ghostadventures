using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Bat_Trigger : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player"))
        {
            SoundTriggerer.TriggerSound(collision.transform.position);
            Debug.Log("박쥐에 부딪혔습니다. 사운드트리거 발생");
        }
    }
}
