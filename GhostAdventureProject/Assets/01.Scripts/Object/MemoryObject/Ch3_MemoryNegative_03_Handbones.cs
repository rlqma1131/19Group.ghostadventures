using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_03_Handbones : MemoryFragment
{
    private bool colected = false;
    public bool Colected => colected;

    public void ActivateBone()
    {
        isScannable = true;
    }

    public override void Scanning()
    {
        base.Scanning();
        colected = true;
        Debug.Log("손모양 뼈 기억 수집 됨");
    }
}
