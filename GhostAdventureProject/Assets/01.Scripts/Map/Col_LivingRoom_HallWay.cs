using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Col_LivingRoom_HallWay : MonoBehaviour
{
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("일단 TV를 켜야 해");
        }
    }
}
