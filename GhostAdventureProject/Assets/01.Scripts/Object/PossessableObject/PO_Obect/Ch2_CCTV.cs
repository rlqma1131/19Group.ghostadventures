using UnityEngine;

public class Ch2_CCTV : BasePossessable
{
    [Header("CCTV 번호")]
    [SerializeField] private int index; // 0 ~ 3

    [Header("CCTV 모니터")]
    [SerializeField] private Ch2_CCTVMonitor monitor;

    [Header("조작키")]
    [SerializeField] private GameObject aKey;
    [SerializeField] private GameObject dKey;

    [Header("하이라이트 애니메이터")]
    [SerializeField] private Animator highlightAnimator;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;

        anim = GetComponentInChildren<Animator>();
    }

    protected override void Update()
    {
        if (monitor.isRevealed) // 기억조각 드러나면 빙의 불가
        {
            InactiveCCTV();
        }

        if (!isPossessed || !hasActivated)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            aKey.SetActive(false);
            dKey.SetActive(false);
        }

        // 좌우 움직임에 따라 모니터화면도 움직이기
        else if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetBool("Right", true);

            if (highlightAnimator != null && highlightAnimator.runtimeAnimatorController != null && highlightAnimator.isActiveAndEnabled)
                highlightAnimator.SetBool("Right", true);

            monitor?.SetMonitorAnimBool(index, "Right", true);
            CheckSolvedPrompt();
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetBool("Right", false);

            if (highlightAnimator != null && highlightAnimator.runtimeAnimatorController != null && highlightAnimator.isActiveAndEnabled)
                highlightAnimator.SetBool("Right", false);

            monitor?.SetMonitorAnimBool(index, "Right", false);
            CheckSolvedPrompt();
        }
    }
    
    void CheckSolvedPrompt()
    {
        if (monitor.SolvedCheck())
        {
            UIManager.Instance.PromptUI.ShowPrompt("CCTV 화면을 확인해야봐야겠어", 2f);
        }
    }

    public void ActivateCCTV()
    {
        hasActivated = true;
    }

    public void InactiveCCTV()
    {
        hasActivated = false;
    }

    public override void OnPossessionEnterComplete() 
    { 
        aKey.SetActive(true);
        dKey.SetActive(true);
    }

    protected override void OnTriggerEnter2D(Collider2D other)
    {
        if (!hasActivated)
            return;

        if (other.CompareTag("Player"))
        {
            SyncHighlightAnimator();
            PlayerInteractSystem.Instance.AddInteractable(gameObject);
        }

        SyncHighlightAnimator();
    }

    public void SyncHighlightAnimator()
    {
        if (highlightAnimator == null || anim == null) return;

        AnimatorStateInfo stateInfo = anim.GetCurrentAnimatorStateInfo(0);

        // 현재 상태 이름을 Hash로 가져오기
        int currentStateHash = stateInfo.shortNameHash;

        // 하이라이트 Animator에 동일한 상태 강제 재생
        highlightAnimator.Play(currentStateHash, 0, stateInfo.normalizedTime);
    }

    public override void CantPossess()
    {
        UIManager.Instance.PromptUI.ShowPrompt("전력이 끊겨있는 것 같아", 2f);
    }
}
