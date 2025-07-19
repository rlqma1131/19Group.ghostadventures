using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;


// 컨디션 * UI * 를 관리하는 스크립트입니다.
public class Person : MonoBehaviour
{
    public PersonCondition currentCondition;
    private PersonCondition lastCondition;
    [SerializeField] private float yPos_UI; // UI의 y포지션 (오브젝트마다 값 다르게 설정)
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
        // if(UI != null)
        //     Destroy(UI);
        
        if (currentCondition == PersonCondition.Vital)
            UI = vitalUI;
        else if (currentCondition == PersonCondition.Normal)
            UI = normalUI;
        else if (currentCondition == PersonCondition.Tired)
            UI = tiredUI;

        Vector3 uiPos = transform.position + Vector3.up * yPos_UI;
        UI.transform.position = uiPos;

        UI.SetActive(true);
    }
}
