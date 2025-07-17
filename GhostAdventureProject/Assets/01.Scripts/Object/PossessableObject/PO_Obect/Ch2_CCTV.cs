using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_CCTV : BasePossessable
{
    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
    }

    public void ActivateCCTV()
    {
        hasActivated = true;
    }
}
