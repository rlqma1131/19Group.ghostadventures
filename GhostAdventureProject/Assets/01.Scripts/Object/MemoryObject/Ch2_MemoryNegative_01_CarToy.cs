using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryNegative_01_CarToy : MemoryFragment
{
    private Animator anim;

    override protected void Start()
    {
        base.Start();
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateCar()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }
}
