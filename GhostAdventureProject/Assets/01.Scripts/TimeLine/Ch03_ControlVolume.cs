using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using UnityEngine.Rendering;

public class Ch03_ControlVolume : MonoBehaviour
{
    public Volume volume;  // 연결할 글로벌 볼륨
    private ColorAdjustments colorAdjustments; // 화면 색상 보정
    private ChromaticAberration chromaticAberration;
    private DepthOfField depthOfField;

    // 목표 색상 (빨간색으로)
    public Color targetColor = new Color(1f, 0f, 0f, 1f);
    public float duration = 3f;

    private void Start()
    {
        if (volume.profile.TryGet<ColorAdjustments>(out colorAdjustments))
        {
            colorAdjustments.colorFilter.overrideState = true;
            
        }
        else
        {
            Debug.LogWarning("Color Adjustments not found.");
        }
        if(volume.profile.TryGet<ChromaticAberration>(out chromaticAberration))
        {
            chromaticAberration.active = true;
        }
        else
        {
            Debug.LogWarning("Chromatic Aberration not found.");
        }
        if(volume.profile.TryGet<DepthOfField>(out depthOfField))
        {
            depthOfField.active = true;
        }
        else
        {
            Debug.LogWarning("Depth of Field not found.");
        }
    }

    //화면을 빨갛게 바꾸는 함수
    public void controlVolume()
    {
        StartCoroutine(FadeToRed());


    }
    public void controlScreen()
    {

        StartCoroutine(FadeToScreen());
    }

    // 화면 색상을 점점 targetColor(빨간색)으로 바꿔줌
    IEnumerator FadeToRed()
    {
        Color startColor = colorAdjustments.colorFilter.value;
        float time = 0f;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = time / duration;
            colorAdjustments.colorFilter.value = Color.Lerp(startColor, targetColor, t);
            yield return null;
        }

        
        colorAdjustments.colorFilter.value = targetColor;
    }

    // 화면 왜곡, 심도효과를 점점 줄이는 코루틴
    IEnumerator FadeToScreen()
    {
        float time = 0f;

        while (time < 5)
        {
            time += Time.deltaTime;
            float t = time / 5;

            chromaticAberration.intensity.value = Mathf.Lerp(1f, 0.1f, t);
            depthOfField.focusDistance.value = Mathf.Lerp(0f, 10f, t);
            yield return null;
        }



    }
}
