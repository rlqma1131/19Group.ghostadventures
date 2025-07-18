using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NormalConditionHandler : PersonConditionHandler
{
    public override float GetQTESpeed() => 1.0f;
    public override float GetSoulCost() => 10f;

    // public override void PerformBehavior(Person person)
    // {
    //     person.animator.SetTrigger("Normal");
    // }
}
