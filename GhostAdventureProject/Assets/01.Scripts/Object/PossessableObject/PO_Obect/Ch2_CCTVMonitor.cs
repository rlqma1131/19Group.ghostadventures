using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_CCTVMonitor : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV 화면 순서대로")]
    [SerializeField] private GameObject[] screens; // 기본 모니터 화면
    [SerializeField] private GameObject[] lazerScreens; // 레이저 모니터 화면

    [Header("가짜 기억 02")]
    [SerializeField] private GameObject memory;

    private Animator[] screenAnimators;

    public bool isRevealed { get; private set; } = false; // 기억조각 처음 한번만 나타내기
    private bool isRevealStarted = false;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        zoomCamera.Priority = 5;

        screenAnimators = new Animator[screens.Length];
        for (int i = 0; i < screens.Length; i++)
        {
            if (screens[i] != null)
                screenAnimators[i] = screens[i].GetComponent<Animator>();
        }
    }

    protected override void Update()
    {
        if (!isPossessed || isRevealStarted)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;
            Unpossess();

            // 기본 모니터 화면으로 전환
            for (int i = 0; i < lazerScreens.Length; i++)
            {
                if (lazerScreens[i] != null)
                {
                    lazerScreens[i].SetActive(false);
                    screens[i].SetActive(true);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 레이저 모니터 화면으로 전환
            for (int i = 0; i < lazerScreens.Length; i++)
            {
                if (lazerScreens[i] != null)
                {
                    lazerScreens[i].SetActive(true);
                    screens[i].SetActive(false);
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // 기본 모니터 화면으로 전환
            for (int i = 0; i < lazerScreens.Length; i++)
            {
                if (lazerScreens[i] != null)
                {
                    lazerScreens[i].SetActive(false);
                    screens[i].SetActive(true);
                }
            }
        }
    }

    // index번 모니터 화면과 연결된 CCTV 설정
    public void SetMonitorAnimBool(int idx, string param, bool value)
    {
        if (idx >= 0 && idx < screens.Length && screens[idx] != null)
        {
            screenAnimators[idx]?.SetBool(param, value);
        }
    }

    public void CheckMemoryUnlockCondition()
    {
        bool[] expected = { true, false, false, true };

        for (int i = 0; i < screens.Length && i < expected.Length; i++)
        {
            if (screens[i] == null)
                return;

            bool current = screenAnimators[i].GetBool("Right");
            if (current != expected[i])
                return; // 하나라도 다르면 조기 종료
        }

        // 전부 조건 만족 시 기억조각 나타남
        // 추후 효과 수정
        isRevealed = true;
        isRevealStarted = true; // 기억 조각 나타나는 동안 조작 불가능
        StartCoroutine(MemoryReveal());
    }

    private IEnumerator MemoryReveal()
    {
        yield return new WaitForSeconds(3f);

        // 효과 재생
        foreach (var animator in screenAnimators)
        {
            if (animator != null)
            {
                animator.SetTrigger("Reveal");
            }
        }

        yield return new WaitForSeconds(4f);

        isRevealStarted = false; // 조작 가능 상태로 복귀
        memory.SetActive(true);
        zoomCamera.Priority = 5;
        Unpossess();
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;

        if(!isRevealed)
            CheckMemoryUnlockCondition();
    }

    public void ActivateCCTVMonitor()
    {
        hasActivated = true;
    }
}
