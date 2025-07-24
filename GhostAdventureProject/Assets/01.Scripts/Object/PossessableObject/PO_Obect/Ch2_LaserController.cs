using UnityEngine;

public class Ch2_LaserController : BasePossessable
{
    [Header("레이저")]
    [SerializeField] private GameObject laser;

    [Header("레이저 스크린")]
    [SerializeField] private GameObject laserScreen;

    [Header("조작키")]
    [SerializeField] private GameObject qKey;
    [SerializeField] private Sprite on;
    [SerializeField] private Sprite off;

    private Animator laserScreenAnimator;
    private SpriteRenderer spriteRenderer;
    protected override void Start()
    {
        isPossessed = false;
        hasActivated = false;
        spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        laserScreenAnimator = laserScreen.GetComponent<Animator>();
    }

    protected override void Update()
    {
        if (!isPossessed)
            return;

        if (Input.GetKeyDown(KeyCode.E))
        {
            Unpossess();
            qKey.SetActive(false);
        }

        // 빙의 상태에서 레이저 On/Off
        if (Input.GetKeyDown(KeyCode.Q))
        {
            bool laserActive = !laser.activeSelf;
            laser.SetActive(laserActive);
            laserScreenAnimator.SetBool("Off", !laserActive);

            spriteRenderer.sprite = laserActive ? on : off;
        }
    }

    public void ActivateController()
    {
        hasActivated = true;
    }

    public override void OnPossessionEnterComplete() 
    {
        qKey.SetActive(true);
    }
}
