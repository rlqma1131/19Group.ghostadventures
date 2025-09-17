using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Global_Death_Camera : MonoBehaviour
{
    //게임오버시 플레이어를 비추는 카메라 우선순위를 최고로 올려주는 스크립트

    [SerializeField] private CinemachineVirtualCamera vcam;

    public void Oncamera()
    {


        vcam.Priority = 50;
    }
    public void OffCamera()
    {

        vcam.Priority = 0;
    }

}
