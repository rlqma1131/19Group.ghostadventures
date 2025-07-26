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
    }

    protected override void OnTriggerEnter2D(Collider2D collision)
    {
        if(collision.CompareTag("Player") && !ChapterEndingManager.Instance.AllCh1CluesCollected())
        {
            UIManager.Instance.PromptUI.ShowPrompt("단서가 부족해...");
        }
    }
}
