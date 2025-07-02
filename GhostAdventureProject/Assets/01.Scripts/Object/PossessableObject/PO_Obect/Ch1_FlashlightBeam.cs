using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ch1_FlashlightBeam : MonoBehaviour
{
    [SerializeField] private GameObject beamVisual;
    [SerializeField] private Color beamColor = Color.white;
    public bool isOn { get; private set; } = false;
    public Color BeamColor => beamColor;

    public void ToggleBeam()
    {
        isOn = !isOn;
        beamVisual.SetActive(isOn);
        
        SpriteRenderer sr = beamVisual.GetComponent<SpriteRenderer>();
        if (sr != null)
            sr.color = beamColor;
    }
}
