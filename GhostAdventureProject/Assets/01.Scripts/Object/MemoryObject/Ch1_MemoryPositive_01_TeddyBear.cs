using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_MemoryPositive_01_TeddyBear : MemoryFragment
{
    [SerializeField] private LockedDoor door;
    void Start()
    {
        isScannable = false;
    }

    public void ActivateTeddyBear()
    {
        isScannable = true;
    }

    public override void AfterScan() 
    { 
        door.SolvePuzzle();
    }
}
