using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Col_LivingRoom_HallWay : MonoBehaviour
{
    BoxCollider2D[] cols;
    [SerializeField] private GameObject icon;

    void Start()
    {
        cols = GetComponentsInChildren<BoxCollider2D>();
        icon.SetActive(false);
    }

    void Update()
    {
        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비"))
        {
            foreach (var col in cols)
            {
                col.isTrigger = true;
            }
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("일단 TV를 켜야 해");
        }

        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비"))
        {
            icon.SetActive(true);  // 트리거로 변경
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비"))
        {
            icon.SetActive(false);  // 트리거로 변경
        }
    }
}
