using Cinemachine;
using UnityEngine;

public class Ch2_CCTVMonitor : BasePossessable
{
    [Header("줌 카메라")]
    [SerializeField] private CinemachineVirtualCamera zoomCamera;

    [Header("CCTV 화면 순서대로")]
    [SerializeField] private Animator[] screenAnimators; // 각 모니터 화면 Animator


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
            screenAnimators[idx].SetBool(param, value);
    }

    public override void OnPossessionEnterComplete()
    {
        zoomCamera.Priority = 20;
    }

    public void ActivateCCTVMonitor()
    {
        hasActivated = true;
    }
}
