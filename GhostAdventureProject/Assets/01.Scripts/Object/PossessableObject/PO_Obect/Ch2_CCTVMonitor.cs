using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_CCTVMonitor : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV 화면 순서대로")]
    [SerializeField] private GameObject[] cctvScreens; // 기본 모니터 화면
    [SerializeField] private GameObject[] laserScreens; // 레이저 모니터 화면

    [Header("가짜 기억 02")]
    [SerializeField] private GameObject memoryH;

    private SpriteRenderer[] cctvScreenSpriteRenderer;
    private SpriteRenderer[] laserScreenSpriteRenderer;

    private Animator[] cctvScreenAnimators;

    public bool isRevealed { get; private set; } = false; // 기억조각 처음 한번만 나타내기
    private bool isRevealStarted = false;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        zoomCamera.Priority = 5;

        cctvScreenSpriteRenderer = new SpriteRenderer[cctvScreens.Length];
        laserScreenSpriteRenderer = new SpriteRenderer[laserScreens.Length];
        cctvScreenAnimators = new Animator[cctvScreens.Length];

        for (int i = 0; i < cctvScreens.Length; i++)
        {
            if (cctvScreens[i] != null)
            {
                cctvScreenSpriteRenderer[i] = cctvScreens[i].GetComponent<SpriteRenderer>();
                cctvScreenAnimators[i] = cctvScreens[i].GetComponent<Animator>();
            }

            if (laserScreens[i] != null)
            {
                laserScreenSpriteRenderer[i] = laserScreens[i].GetComponent<SpriteRenderer>();
            }
        }

        for (int i = 0; i < laserScreens.Length; i++)
        {
            if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
            {
                cctvScreenSpriteRenderer[i].enabled = true;
                laserScreenSpriteRenderer[i].enabled = false;
            }
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
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
                {
                    cctvScreenSpriteRenderer[i].enabled = true;
                    laserScreenSpriteRenderer[i].enabled = false;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.D))
        {
            // 레이저 모니터 화면으로 전환
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreenSpriteRenderer[i] != null && cctvScreenSpriteRenderer[i] != null)
                {
                    cctvScreenSpriteRenderer[i].enabled = false;
                    laserScreenSpriteRenderer[i].enabled = true;
                }
            }
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            // 기본 모니터 화면으로 전환
            for (int i = 0; i < laserScreens.Length; i++)
            {
                if (laserScreens[i] != null)
                {
                    laserScreenSpriteRenderer[i].enabled = false;
                    cctvScreenSpriteRenderer[i].enabled = true;
                }
            }
        }
    }

    // index번 모니터 화면과 연결된 CCTV 설정
    public void SetMonitorAnimBool(int idx, string param, bool value)
    {
        if (idx >= 0 && idx < cctvScreens.Length && cctvScreens[idx] != null)
        {
            cctvScreenAnimators[idx]?.SetBool(param, value);
        }
    }

    public void CheckMemoryUnlockCondition()
    {
        bool[] expected = { true, false, false, true };

        for (int i = 0; i < cctvScreens.Length && i < expected.Length; i++)
        {
            if (cctvScreens[i] == null)
                return;

            bool current = cctvScreenAnimators[i].GetBool("Right");
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
        foreach (var animator in cctvScreenAnimators)
        {
            if (animator != null)
            {
                animator.SetTrigger("Reveal");
            }
        }

        yield return new WaitForSeconds(4f);

        memoryH.SetActive(true);
        hasActivated = false; // 기억 스캔 전까지 빙의 불가
        zoomCamera.Priority = 5;

        isRevealStarted = false; // 조작 가능 상태로 복귀
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
