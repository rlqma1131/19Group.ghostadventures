using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_MemoryPositive_01_TeddyBear : MemoryFragment
{

    public bool Completed_TeddyBear = false;

    void Start()
    {
        isScannable = false;
    }

    public void ActivateTeddyBear()
    {
        isScannable = true;
    }

    public override void AfterScan() 
    {
        isScannable = false;
        Completed_TeddyBear = true;
        PuzzleStateManager.Instance.MarkPuzzleSolved("곰인형");
        
    }
    protected override void PlusAction()
    {
        UIManager.Instance.PromptUI.ShowPrompt_2("맞아 이건 내 기억이야", "여기서 볼 일은 끝난거 같아");    
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !ChapterEndingManager.Instance.AllCh1CluesCollected())
        {
            UIManager.Instance.PromptUI.ShowPrompt("단서가 부족해...");
        }
    }
}
