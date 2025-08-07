using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Col_LivingRoom_HallWay : MonoBehaviour
{
    BoxCollider2D[] cols;
    BoxCollider2D maincol;
    [SerializeField] private GameObject icon;
    private bool onTrigger;

    void Start()
    {
        maincol = GetComponent<BoxCollider2D>();
        // cols = GetComponentsInChildren<BoxCollider2D>();
        // icon.SetActive(false);
    }

    void Update()
    {
        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비") && !onTrigger)
        {
            maincol.isTrigger = true;
            // foreach (var col in cols)
            // {
            //     col.isTrigger = true;
                
            // }
            onTrigger = true;
        }
    }
    void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("시계"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("일단 TV를 켜야 해");
        }
        if(icon == null) return;

        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비"))
        {
            
            icon.SetActive(true);  // 트리거로 변경
        }
    }

    void OnTriggerExit2D(Collider2D collision)
    {
        if(icon == null) return;
        if(PuzzleStateManager.Instance.IsPuzzleSolved("티비"))
        {
            icon.SetActive(false);  // 트리거로 변경
        }
    }
}
