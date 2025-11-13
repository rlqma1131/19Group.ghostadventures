using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryFake_02_CCTV : MemoryFragment
{
    [SerializeField] private Ch2_CCTVMonitor cctvMonitor;

    public void ActivateCCTV()
    {
        SetScannable(true);
    }

    public override void Scanning()
    {
        cctvMonitor.ActivateCCTVMonitor();

        base.Scanning();
    }
}
