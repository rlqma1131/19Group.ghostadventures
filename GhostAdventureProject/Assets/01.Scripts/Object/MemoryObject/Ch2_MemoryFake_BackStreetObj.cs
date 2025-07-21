using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryFake_BackStreetObj : MemoryFragment
{
    void Start()
    {
        isScannable = false;
    }

    public void ActivateBackStreetObj()
    {
        isScannable = true;
    }
}
