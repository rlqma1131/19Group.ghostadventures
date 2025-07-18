using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
public enum PersonCondition
{
    Vital,   // 활력
    Normal,  // 보통
    Tired    // 피곤함
}

public abstract class PersonConditionHandler
{
    public abstract float GetQTESpeed();
    public abstract float GetSoulCost();
    public virtual void ShowCondition(Vector3 personPos, GameObject conditionUI)
    {
        Vector3 uiPos = personPos;
        uiPos.y += 3f;
        conditionUI.transform.position = uiPos;
        conditionUI.SetActive(true);
    }
}
