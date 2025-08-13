using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_03_Handbones : MemoryFragment
{
    private bool colected = false;
    public bool Colected => colected;

    void Start()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        base.AfterScan();
        colected = true;
        Debug.Log("손모양 뼈 기억 수집 됨");
    }
}
