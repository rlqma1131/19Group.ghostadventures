using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Plate : BasePossessable
{
    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            TriggerPlateEvent();
        }
    }

    private void TriggerPlateEvent()
    {

    }
}