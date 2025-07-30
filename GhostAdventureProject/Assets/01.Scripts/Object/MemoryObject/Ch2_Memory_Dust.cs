using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_Memory_Dust : MemoryFragment
{
    [SerializeField] private GameObject letterE;
    public override void AfterScan()
    {
        letterE.SetActive(true);
    }
}
