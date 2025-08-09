using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_02_Paper : MemoryFragment
{
    private bool colected = false;
    public bool Colected => colected;

    public void ActivatePaper()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        colected = true;
        isScannable = false;
    }
}
