using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryPositive_Mirror : MemoryFragment
{
    private Ch3_Scanner scanner;

    void Start()
    {
        scanner = FindObjectOfType<Ch3_Scanner>();
    }

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
