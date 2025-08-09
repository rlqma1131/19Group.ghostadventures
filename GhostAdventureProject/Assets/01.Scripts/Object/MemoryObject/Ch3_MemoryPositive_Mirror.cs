using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryPositive_Mirror : MemoryFragment
{
    [SerializeField] private Ch3_Scanner scanner;

    public void ActivateObj()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        isScannable = false;
        scanner.ActiveScanner();
    }
}
