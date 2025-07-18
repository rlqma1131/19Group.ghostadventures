using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryFake_02_CCTV : MemoryFragment
{
    void Start()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        isScannable = false;
    }
}
