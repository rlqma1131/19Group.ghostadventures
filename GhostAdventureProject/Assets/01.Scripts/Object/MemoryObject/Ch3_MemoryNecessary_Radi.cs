using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNecessary_Radi : MemoryFragment
{
    public void ActivateRadi()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        isScannable = false;
    }
}
