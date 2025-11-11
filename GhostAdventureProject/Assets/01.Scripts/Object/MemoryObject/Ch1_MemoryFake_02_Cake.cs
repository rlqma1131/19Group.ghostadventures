using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.SceneManagement;

public class Ch1_MemoryFake_02_Cake : MemoryFragment
{
    private Animator anim;
    [SerializeField] private GameObject _particleSystem;
    [SerializeField] private GameObject _light;

    override protected void Start()
    {
        base.Start();
        isScannable = false;
        anim = GetComponentInChildren<Animator>();
    }

    public void ActivateCake()
    {
        isScannable = true;
        if (TryGetComponent(out UniqueId uid))
            SaveManager.SetMemoryFragmentScannable(uid.Id, isScannable);
    }

    public override void Scanning()
    {
        anim.SetBool("Show", true);

        ChapterEndingManager.Instance.CollectCh1Clue("H");

        AfterScanEffect(); // 애니메이션 재생 후 효과 실행

        Invoke("HighlightOff", 1f);
    }

    public override void AfterScan()
    {
        SaveManager.SaveWhenScanAfter(data.memoryID, data.memoryTitle,
            SceneManager.GetActiveScene().name,
            GameManager.Instance.PlayerObj.transform.position,
            checkpointId: data.memoryID,
            autosave: true);
    }

    void HighlightOff()
    {
        Highlight.SetActive(false); // 하이라이트 비활성화
    }

    protected override void PlusAction()
    {
        UIManager.Instance.PromptUI.ShowPrompt("H");
    }

    private void AfterScanEffect()
    {
        _particleSystem.SetActive(true);
        _light.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        base.OnTriggerEnter2D(other);
        if(other.CompareTag("Player") && !SaveManager.IsPuzzleSolved("후라이팬"))
        {
            UIManager.Instance.PromptUI.ShowPrompt("쥐를 먼저 쫒아내야겠어");
        }
        else if(other.CompareTag("Player") && SaveManager.IsPuzzleSolved("후라이팬"))
        {
            TutorialManager.Instance.Show(TutorialStep.Cake_Prompt);
        } 
    }
}
