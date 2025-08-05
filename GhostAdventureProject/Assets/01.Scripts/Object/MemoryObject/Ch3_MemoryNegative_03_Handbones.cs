using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_03_Handbones : MemoryFragment
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
