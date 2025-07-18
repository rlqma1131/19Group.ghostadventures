using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VitalConditionHandler : PersonConditionHandler
{
    public override float GetQTESpeed() => 1.5f;
    public override float GetSoulCost() => 5f;

    private void Update() 
    {
        // guard.transform.position 
    }

    // public override void PerformBehavior(Person person)
    // {
    //     person.animator.SetTrigger("Energetic");
    //     // 빠르게 행동하거나 특수 스킬을 사용할 수도 있음
    // }
}
