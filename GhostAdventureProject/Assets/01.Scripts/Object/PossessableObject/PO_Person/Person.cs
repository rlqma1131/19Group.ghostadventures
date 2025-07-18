using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Person : MonoBehaviour
{
    public static PersonCondition currentCondition;
    private PersonCondition lastCondition;
    public GameObject UI;
    public GameObject vitalUI;
    public GameObject normalUI;
    public GameObject tiredUI;

    void Start()
    {
        lastCondition = currentCondition;
        ShowConditionUI();
    }
    void Update()
    {
        ShowConditionUI();
    }    

    void ShowConditionUI()
    {
        if (currentCondition == PersonCondition.Vital)
            UI = vitalUI;
        else if (currentCondition == PersonCondition.Normal)
            UI = normalUI;
        else if (currentCondition == PersonCondition.Tired)
            UI = tiredUI;

        Vector3 uiPos = transform.position + Vector3.up * 2.5f;
        UI.transform.position = uiPos;

        UI.SetActive(true);
    }
}
