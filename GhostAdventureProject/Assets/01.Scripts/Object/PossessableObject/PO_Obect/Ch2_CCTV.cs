using UnityEngine;

public class Ch2_CCTV : BasePossessable
{
    [Header("CCTV 번호")]
    [SerializeField] private int index; // 0 ~ 3

    [Header("CCTV 모니터")]
    [SerializeField] private Ch2_CCTVMonitor monitor;

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
        }
        // 좌우 움직임에 따라 모니터화면도 움직이기
        else if (Input.GetKeyDown(KeyCode.D))
        {
            anim.SetBool("Right", true);
            monitor?.SetMonitorAnimBool(index, "Right", true);
        }
        else if (Input.GetKeyDown(KeyCode.A))
        {
            anim.SetBool("Right", false);
            monitor?.SetMonitorAnimBool(index, "Right", false);
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
}
