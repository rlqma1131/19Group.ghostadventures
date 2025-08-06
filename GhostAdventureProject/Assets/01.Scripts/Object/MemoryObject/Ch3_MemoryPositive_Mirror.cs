using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryPositive_Mirror : MemoryFragment
{
    // Start is called before the first frame update
    public void ActivateObj()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        isScannable = false;
    }
}
