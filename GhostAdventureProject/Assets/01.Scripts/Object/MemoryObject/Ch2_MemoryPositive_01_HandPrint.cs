using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryPositive_01_HandPrint : MemoryFragment
{
    public Ch2_ClearDoor exit;

    public void ActivateHandPrint()
    {
        isScannable = true;
    }

    public override void AfterScan() 
    {
        exit.ActivateClearDoor();
    }
}
