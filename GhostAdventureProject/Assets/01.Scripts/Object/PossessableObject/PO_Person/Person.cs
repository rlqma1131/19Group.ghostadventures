using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Person : MonoBehaviour
{
    public PersonCondition currentCondition;
    private PersonCondition lastCondition;
    public GameObject UI;
    public GameObject vitalUI; // 프리팹
    public GameObject normalUI; // 프리팹
    public GameObject tiredUI; // 프리팹

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

        Vector3 uiPos = transform.position + Vector3.up * 3f;
        UI.transform.position = uiPos;

        UI.SetActive(true);
    }
}
