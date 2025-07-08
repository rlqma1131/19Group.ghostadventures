using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class PostProcessController : MonoBehaviour
{
    public PostProcessVolume volume;
    private ColorGrading colorGrading;

    public float Saturation
    {
        get
        {
            if (volume.profile.TryGetSettings(out colorGrading))
                return colorGrading.saturation.value;
            return 0f;
        }
        set
        {
            if (volume.profile.TryGetSettings(out colorGrading))
                colorGrading.saturation.value = value;
        }
    }
}
