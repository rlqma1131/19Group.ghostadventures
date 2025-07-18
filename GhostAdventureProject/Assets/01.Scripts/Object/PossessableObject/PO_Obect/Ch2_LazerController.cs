using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch2_LazerController : BasePossessable
{
    [Header("ID")]
    [SerializeField] private int id; // 0 ~ 3

    [Header("레이저")]
    [SerializeField] private GameObject[] lazers; // 0 ~ 3

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        // 빙의 상태에서 레이저 On/Off
        if (Input.GetKeyDown(KeyCode.Q))
        {
            lazers[id].SetActive(!lazers[id].activeSelf);
        }
    }

    public void ActivateController()
    {
        hasActivated = true;
    }
}
