using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch3_Xray : BasePossessable
{
    [Header("X-Ray 화면")]
    [SerializeField] private Ch3_Xray_Monitor monitor;

    [Header("사진")]
    [SerializeField] private Sprite photo01;
    [SerializeField] private Sprite photo02;
    [SerializeField] private Sprite photo03;
    [SerializeField] private Sprite photo04;

    [Header("조작키")]
    [SerializeField] private GameObject aKey;
    [SerializeField] private GameObject dKey;

    protected override void Update()
    {
        base.Update();

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            aKey.SetActive(false);
            dKey.SetActive(false);
        }

        // 좌우 움직임에 따라 photo 화면도 움직이기
        else if (Input.GetKeyDown(KeyCode.D))
        {
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
        }
    }

    public override void OnPossessionEnterComplete()
    {
        aKey.SetActive(true);
        dKey.SetActive(true);
    }
}
