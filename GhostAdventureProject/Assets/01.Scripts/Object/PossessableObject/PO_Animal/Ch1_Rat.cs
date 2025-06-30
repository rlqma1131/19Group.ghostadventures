using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Rat : MoveBasePossessable
{
    void Start()
    {
        isCompleted = false;
    }

    protected override void Update()
    {
        base.Update();

        // if(상호작용 인풋)
        //     OnActivate();

    }

    public void OnActivate()
    {
        // 고유 기능 로직
    }
}
