using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_FakeDoll : MemoryFragment
{
    // 스캔시 사신트리거 발동

    private Animator anim;

    void Start()
    {
        isScannable = true;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateFakeDoll()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        // isScannable = false;
        // SoundManager.Instance.PlaySFX(isFall);
        SoundTriggerer.TriggerSound(transform.position);
        Debug.Log("Fake스캔 완료. 사운드트리거 발동");
    }

}
