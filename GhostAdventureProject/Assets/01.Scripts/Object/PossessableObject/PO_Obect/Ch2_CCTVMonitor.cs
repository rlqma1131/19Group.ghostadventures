using Cinemachine;
using System.Collections;
using UnityEngine;

public class Ch2_CCTVMonitor : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV 화면 순서대로")]
    [SerializeField] private Animator[] screenAnimators; // 각 모니터 화면 Animator

    [Header("가짜 기억 02")]
    [SerializeField] private GameObject memory;


    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        zoomCamera.Priority = 5;
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            zoomCamera.Priority = 5;
            Unpossess();
        }
    }

    // index번 모니터 화면과 연결된 CCTV 설정
    public void SetMonitorAnimBool(int idx, string param, bool value)
    {
        if (idx >= 0 && idx < screenAnimators.Length && screenAnimators[idx] != null)
        {
            screenAnimators[idx].SetBool(param, value);
        }
    }

    public void CheckMemoryUnlockCondition()
    {
        bool[] expected = { true, false, false, true };

        for (int i = 0; i < screenAnimators.Length && i < expected.Length; i++)
        {
            if (screenAnimators[i] == null)
                return;

            bool current = screenAnimators[i].GetBool("Right");
            if (current != expected[i])
                return; // 하나라도 다르면 조기 종료
        }

        // 전부 조건 만족 시 기억조각 나타남
        // 추후 효과 수정
        StartCoroutine(MemoryReveal());
    }

    private IEnumerator MemoryReveal()
    {
        // 효과 재생
        foreach (var animator in screenAnimators)
        {
            if (animator != null)
            {
                animator.SetTrigger("Reveal");
            }
        }

        yield return new WaitForSeconds(3f);

        memory.SetActive(true);
        zoomCamera.Priority = 5;
        Unpossess();
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;
        CheckMemoryUnlockCondition();
    }

    public void ActivateCCTVMonitor()
    {
        hasActivated = true;
    }
}
