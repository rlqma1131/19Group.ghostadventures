using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNegative_02_Paper : MemoryFragment
{
    public void ActivatePaper()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void AfterScan()
    {
        base.AfterScan();
        Debug.Log("종이 기억 수집 됨");
    }
}
