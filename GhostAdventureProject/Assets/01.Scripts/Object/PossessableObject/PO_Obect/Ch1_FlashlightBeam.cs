using UnityEngine;

public class Ch1_FlashlightBeam : MonoBehaviour
{
    [SerializeField] private GameObject beamVisual;
    public bool isOn { get; private set; } = false;

    public void ToggleBeam()
    {
        isOn = !isOn;
        beamVisual.SetActive(isOn);
    }
}
