using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_MemoryNecessary_Radi : MemoryFragment
{
    [SerializeField] private GameObject b1fDoor;
    [SerializeField] private LockedDoor lockedDoor;
    public void ActivateRadi()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        isScannable = false;
        b1fDoor.SetActive(true);
        lockedDoor.SolvePuzzle();
    }
}
