using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_Mirror : MonoBehaviour
{
    [SerializeField] private Ch1_Shower shower;
    [SerializeField] private CanvasGroup fogCanvas;
    [SerializeField] private GameObject wLetter;
    [SerializeField] private float fogDuration = 5f;

    private float fogTime = 0f;
    private bool revealed = false;

    private void Update()
    {
        if(fogCanvas == null || shower == null || revealed)
            return;

        if (shower.IsHotWater)
        {
            fogTime += Time.deltaTime;
            fogCanvas.alpha = Mathf.Clamp01(fogTime / fogDuration);

            if (fogTime >= fogDuration)
            {
                fogCanvas.alpha = 1f;
                wLetter.SetActive(true);
                revealed = true;
            }
        }
        else
        {
            fogTime = 0f;                      // 시간 초기화
            fogCanvas.alpha = 0f;
            wLetter.SetActive(false);
        }
    }
}
