using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    private Animator anim;
    private ParticleSystem particleSystem;
    private Light2D light;
    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
        particleSystem = GetComponentInChildren<ParticleSystem>();
        light = GetComponentInChildren<Light2D>();
    }

    public void ActivateCake()
    {
        isScannable = true;
    }

    public override void AfterScan()
    {
        anim.SetTrigger("Show");
        Invoke(nameof(AfterScanEffect), 8f); // 애니메이션 재생 후 효과 실행
        isScannable = false;
        ChapterEndingManager.Instance.CollectCh1Clue("H");
    }
    private void AfterScanEffect()
    {

    
        particleSystem.Play();
        light.enabled = true;
    }
}
