using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiredConditionHandler : PersonConditionHandler
{

    public override float GetQTESpeed() => 0.6f;
    public override float GetSoulCost() => 20f;

    public void ShowCondition(Vector3 personPos, GameObject conditionUI)
    {
        Vector3 uiPos = personPos;
        uiPos.y += 3f;
        conditionUI.transform.position = uiPos;
        conditionUI.SetActive(true);
    }
}
