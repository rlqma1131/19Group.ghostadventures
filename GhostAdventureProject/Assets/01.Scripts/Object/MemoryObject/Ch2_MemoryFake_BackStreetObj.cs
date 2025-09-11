using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_MemoryFake_BackStreetObj : MemoryFragment
{
    [SerializeField] private GameObject drawingClue2;
    public override void Scanning()
    {
        drawingClue2.SetActive(true);

        base.Scanning();
    }
}
