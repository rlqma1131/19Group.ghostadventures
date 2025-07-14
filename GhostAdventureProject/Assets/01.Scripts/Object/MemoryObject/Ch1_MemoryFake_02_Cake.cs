using UnityEngine;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    private Animator anim;

    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateCake()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        anim.SetTrigger("Show");
        isScannable = false;
        ChapterEndingManager.Instance.CollectCh1Clue("H");
    }
}
