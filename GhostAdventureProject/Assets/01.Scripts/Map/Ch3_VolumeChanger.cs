using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch3_VolumeChanger : MonoBehaviour
{


    [SerializeField] private Volume volume;
    private ColorAdjustments colorAdjustments;

    private void Start()
    {
        if (volume != null && volume.profile.TryGet(out colorAdjustments))
        {
            // 시작할 땐 꺼져 있도록
            colorAdjustments.postExposure.overrideState = false;
        }
    }
    private void OnTriggerEnter2D(Collider2D other) 
    {
        if (other.CompareTag("Player") && colorAdjustments != null)
        {
        Debug.Log($"OnTriggerEnter2D: {other.name} - {other.CompareTag("Player")}");
            colorAdjustments.postExposure.overrideState = true; // Post Exposure 적용
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player") && colorAdjustments != null)
        {
            colorAdjustments.postExposure.overrideState = false; // Post Exposure 해제
        }
    }
}


