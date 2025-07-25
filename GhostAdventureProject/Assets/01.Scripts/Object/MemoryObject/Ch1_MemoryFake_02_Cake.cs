using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    private Animator anim;
    private ParticleSystem _particleSystem;
    private Light2D _light;

    void Start()
    {
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
        _particleSystem = GetComponentInChildren<ParticleSystem>();
        _light = GetComponentInChildren<Light2D>();
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

    
        _particleSystem.Play();
        _light.enabled = true;
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Player") && !PuzzleStateManager.Instance.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("쥐를 먼저 쫒아내야겠어");
        }
        else if(other.CompareTag("Player") && PuzzleStateManager.Instance.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt_2("쥐는 어디로 갔을라나?", "아무튼 이제 케잌을 살펴보자");
        } 
    }
}
