using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    [SerializeField] private Animator hilightAnim;
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
        isScannable = false;
        anim.SetTrigger("Show");
        highlight.SetActive(false); // 하이라이트 비활성화

        AfterScanEffect(); // 애니메이션 재생 후 효과 실행
        ChapterEndingManager.Instance.CollectCh1Clue("H");
        Debug.Log("[Cake] AfterScan 호출됨");
    }

    protected override void PlusAction()
    {
        UIManager.Instance.PromptUI.ShowPrompt("H");
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
            TutorialManager.Instance.Show(TutorialStep.Cake_Prompt);
        } 
    }
}
