using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Clock : BasePossessable
{
    [SerializeField] private Transform hourHand;
    [SerializeField] private Transform minuteHand;
    [SerializeField] private Camera zoomCamera;
    [SerializeField] private GameObject tvObject;
    
    private bool isControlMode = false;

    private int hour = 0;
    private int minute = 0;

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
        {
            
        }
    }
}
