using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryNegative_01_CarToy : MemoryFragment
{
    private Animator anim;

    void Start()
    {
        isScannable = true;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateCake()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        // anim.SetTrigger("Show");
        isScannable = false;
        // ChapterEndingManager.Instance.CollectCh1Clue("H");
    }
}
