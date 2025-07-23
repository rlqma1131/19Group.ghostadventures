using UnityEngine;

public class Ch2_LaserController : BasePossessable
{
    [Header("레이저")]
    [SerializeField] private GameObject laser;

    [Header("레이저 스크린")]
    [SerializeField] private GameObject laserScreen;

    [Header("조작키")]
    [SerializeField] private GameObject qKey;

    private Animator laserScreenAnimator;

    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;

        laserScreenAnimator = laserScreen.GetComponent<Animator>();
    }

    protected override void Update()
    {
        base.Update();

        if (!isPossessed)
            return;

        // 빙의 상태에서 레이저 On/Off
        if (Input.GetKeyDown(KeyCode.Q))
        {
            laser.SetActive(!laser.activeSelf);
            laserScreenAnimator.SetBool("Off", !laser.activeSelf);
        }
    }

    public void ActivateController()
    {
        hasActivated = true;
    }
}
