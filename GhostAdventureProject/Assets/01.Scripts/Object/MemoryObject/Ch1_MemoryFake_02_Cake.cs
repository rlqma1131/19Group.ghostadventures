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
        // 코루틴
        anim.SetTrigger("Show");
        ChapterEndingManager.Instance.CollectCh1Clue("H");
    }
}
